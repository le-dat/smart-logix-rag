using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Controllers
{
    /// <summary>
    /// Secure AI Gateway Controller: proxies risk prediction and RAG chat requests
    /// to the Python FastAPI AI engine, enforcing JWT authentication and logging results to SQL.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SmartLogixDbContext _context;
        private readonly ILogger<AiController> _logger;

        public AiController(IHttpClientFactory httpClientFactory, SmartLogixDbContext context, ILogger<AiController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        // ─── Risk Prediction Proxy ────────────────────────────────────────────

        /// <summary>
        /// Proxies a risk prediction request to Python XGBoost service, saves the
        /// RiskScore result to SQL, and returns the prediction to the client.
        /// POST /api/ai/predict
        /// </summary>
        [HttpPost("predict")]
        public async Task<IActionResult> PredictRisk([FromBody] JsonElement payload)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FastApi");
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/v1/risk/predict", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Python AI predict returned {Status}: {Body}", response.StatusCode, body);
                    return StatusCode((int)response.StatusCode, new { message = "AI engine error", detail = body });
                }

                // Parse prediction and optionally save to RiskScores table
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    var shipmentIdExists = payload.TryGetProperty("shipment_id", out var shipIdProp);
                    if (shipmentIdExists && shipIdProp.ValueKind != JsonValueKind.Null)
                    {
                        var shipmentId = shipIdProp.GetInt32();
                        var riskScore = new RiskScore
                        {
                            ShipmentId = shipmentId,
                            Score = (decimal)(root.TryGetProperty("risk_score", out var rs) ? rs.GetDouble() : 0),
                            RiskLevel = root.TryGetProperty("risk_level", out var rl) ? rl.GetString() ?? "Unknown" : "Unknown",
                            Factors = root.TryGetProperty("contributing_factors", out var cf) ? cf.GetRawText() : "{}",
                            CalculatedAt = DateTime.UtcNow
                        };

                        _context.RiskScores.Add(riskScore);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("RiskScore saved for ShipmentId={ShipmentId}", shipmentId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not persist RiskScore to database (non-fatal).");
                }

                return Content(body, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to proxy risk prediction to Python AI service.");
                return StatusCode(500, new { message = "Failed to reach AI engine.", detail = ex.Message });
            }
        }

        // ─── Chat Proxy (non-streaming fallback) ─────────────────────────────

        /// <summary>
        /// Proxies a chat query to Python RAG service and logs the interaction to ChatLogs.
        /// POST /api/ai/chat
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] JsonElement payload)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FastApi");
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/v1/chat/", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Python AI chat returned {Status}: {Body}", response.StatusCode, body);
                    return StatusCode((int)response.StatusCode, new { message = "AI engine error", detail = body });
                }

                // Log chat interaction to SQL ChatLogs table
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    var username = User?.Identity?.Name ?? "anonymous";
                    var promptText = payload.TryGetProperty("prompt", out var p) ? p.GetString() ?? "" : "";
                    var responseText = root.TryGetProperty("response", out var r) ? r.GetString() ?? "" : "";
                    var providerText = root.TryGetProperty("provider_used", out var pv) ? pv.GetString() ?? "Unknown" : "Unknown";

                    _context.ChatLogs.Add(new ChatLog
                    {
                        UserId = username,
                        Prompt = promptText,
                        Response = responseText,
                        LLMProvider = providerText,
                        Rating = null,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not persist ChatLog to database (non-fatal).");
                }

                return Content(body, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to proxy chat request to Python AI service.");
                return StatusCode(500, new { message = "Failed to reach AI engine.", detail = ex.Message });
            }
        }

        // ─── Streaming Chat Proxy (SSE) ───────────────────────────────────────

        /// <summary>
        /// Proxies a streaming chat request to Python RAG SSE endpoint, forwarding
        /// SSE chunks in real-time to the Vue 3 client. Logs final response to ChatLogs.
        /// POST /api/ai/chat/stream
        /// </summary>
        [HttpPost("chat/stream")]
        public async Task StreamChat([FromBody] JsonElement payload, CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";
            Response.Headers["X-Accel-Buffering"] = "no"; // Disable nginx buffering

            var username = User?.Identity?.Name ?? "anonymous";
            var promptText = payload.TryGetProperty("prompt", out var p) ? p.GetString() ?? "" : "";
            var fullResponseBuilder = new StringBuilder();
            var providerUsed = "Unknown";

            try
            {
                var client = _httpClientFactory.CreateClient("FastApi");
                var json = JsonSerializer.Serialize(payload);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                using var pyResponse = await client.PostAsync("/api/v1/chat/stream", requestContent, cancellationToken);

                if (!pyResponse.IsSuccessStatusCode)
                {
                    var errBody = await pyResponse.Content.ReadAsStringAsync(cancellationToken);
                    var errorEvent = $"data: {JsonSerializer.Serialize(new { type = "error", message = errBody })}\n\n";
                    await Response.WriteAsync(errorEvent, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                    return;
                }

                using var stream = await pyResponse.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new System.IO.StreamReader(stream);

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (line == null) break;

                    // Forward SSE lines verbatim
                    await Response.WriteAsync(line + "\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);

                    // Capture final data chunks for logging
                    if (line.StartsWith("data: "))
                    {
                        try
                        {
                            var dataJson = line.Substring(6);
                            using var doc = JsonDocument.Parse(dataJson);
                            var root = doc.RootElement;

                            if (root.TryGetProperty("token", out var token))
                                fullResponseBuilder.Append(token.GetString());

                            if (root.TryGetProperty("provider", out var pv))
                                providerUsed = pv.GetString() ?? "Unknown";
                        }
                        catch { /* non-parseable SSE line, skip */ }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SSE stream cancelled by client.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding SSE stream from Python AI service.");
                try
                {
                    var errorEvent = $"data: {JsonSerializer.Serialize(new { type = "error", message = ex.Message })}\n\n";
                    await Response.WriteAsync(errorEvent, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch { /* client disconnected */ }
            }
            finally
            {
                // Log full accumulated chat response to SQL
                try
                {
                    var finalText = fullResponseBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(finalText) || !string.IsNullOrWhiteSpace(promptText))
                    {
                        _context.ChatLogs.Add(new ChatLog
                        {
                            UserId = username,
                            Prompt = promptText,
                            Response = finalText,
                            LLMProvider = providerUsed,
                            Rating = null,
                            CreatedAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync(CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not persist streaming ChatLog (non-fatal).");
                }
            }
        }
    }
}
