using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartLogix.WebApi.Services;

namespace SmartLogix.WebApi.Controllers
{
    /// <summary>
    /// Secure AI Gateway Controller: proxies risk prediction and RAG chat requests
    /// to the Python FastAPI AI engine, enforcing JWT authentication via IAiService.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly ILogger<AiController> _logger;

        public AiController(IAiService aiService, ILogger<AiController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        // ─── Risk Prediction Proxy ────────────────────────────────────────────

        /// <summary>
        /// Proxies a risk prediction request to Python XGBoost service via IAiService.
        /// POST /api/ai/predict
        /// </summary>
        [HttpPost("predict")]
        public async Task<IActionResult> PredictRisk([FromBody] JsonElement payload)
        {
            var (isSuccess, responseContent, errorMessage) = await _aiService.PredictRiskAsync(payload);
            if (!isSuccess)
            {
                return StatusCode(500, new { message = errorMessage ?? "AI engine error", detail = responseContent });
            }

            return Content(responseContent, "application/json");
        }

        // ─── Chat Proxy (non-streaming fallback) ─────────────────────────────

        /// <summary>
        /// Proxies a chat query to Python RAG service via IAiService.
        /// POST /api/ai/chat
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] JsonElement payload)
        {
            var username = User?.Identity?.Name ?? "anonymous";
            var (isSuccess, responseContent, errorMessage) = await _aiService.ChatAsync(payload, username);
            if (!isSuccess)
            {
                return StatusCode(500, new { message = errorMessage ?? "AI engine error", detail = responseContent });
            }

            return Content(responseContent, "application/json");
        }

        // ─── Streaming Chat Proxy (SSE) ───────────────────────────────────────

        /// <summary>
        /// Proxies a streaming chat request to Python RAG SSE endpoint via IAiService.
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

            try
            {
                await foreach (var line in _aiService.StreamChatAsync(payload, username, cancellationToken))
                {
                    await Response.WriteAsync(line + "\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
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
        }
    }
}
