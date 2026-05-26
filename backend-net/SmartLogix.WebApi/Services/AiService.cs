using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Services
{
    public class AiService : IAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SmartLogixDbContext _context;
        private readonly ILogger<AiService> _logger;

        public AiService(IHttpClientFactory httpClientFactory, SmartLogixDbContext context, ILogger<AiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        // ─── Risk Prediction ──────────────────────────────────────────────────

        public async Task<(bool IsSuccess, string ResponseContent, string? ErrorMessage)> PredictRiskAsync(JsonElement payload)
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
                    return (false, body, $"AI engine error: {response.StatusCode}");
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

                return (true, body, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to proxy risk prediction to Python AI service.");
                return (false, string.Empty, ex.Message);
            }
        }

        // ─── Chat (Non-Streaming) ─────────────────────────────────────────────

        public async Task<(bool IsSuccess, string ResponseContent, string? ErrorMessage)> ChatAsync(JsonElement payload, string username)
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
                    return (false, body, $"AI engine error: {response.StatusCode}");
                }

                // Log chat interaction to SQL ChatLogs table
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

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

                return (true, body, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to proxy chat request to Python AI service.");
                return (false, string.Empty, ex.Message);
            }
        }

        // ─── Chat Streaming (SSE) ─────────────────────────────────────────────

        public async IAsyncEnumerable<string> StreamChatAsync(JsonElement payload, string username, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var promptText = payload.TryGetProperty("prompt", out var p) ? p.GetString() ?? "" : "";
            var fullResponseBuilder = new StringBuilder();
            var providerUsed = "Unknown";

            HttpResponseMessage? pyResponse = null;
            Stream? stream = null;
            StreamReader? reader = null;

            try
            {
                var client = _httpClientFactory.CreateClient("FastApi");
                var json = JsonSerializer.Serialize(payload);
                var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

                pyResponse = await client.PostAsync("/api/v1/chat/stream", requestContent, cancellationToken);

                if (!pyResponse.IsSuccessStatusCode)
                {
                    var errBody = await pyResponse.Content.ReadAsStringAsync(cancellationToken);
                    yield return $"data: {JsonSerializer.Serialize(new { type = "error", message = errBody })}\n\n";
                    yield break;
                }

                stream = await pyResponse.Content.ReadAsStreamAsync(cancellationToken);
                reader = new StreamReader(stream);

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (line == null) break;

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

                    yield return line;
                }
            }
            finally
            {
                // Dispose HTTP stream readers
                reader?.Dispose();
                stream?.Dispose();
                pyResponse?.Dispose();

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
