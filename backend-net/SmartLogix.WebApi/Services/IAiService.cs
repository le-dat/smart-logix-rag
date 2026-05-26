using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartLogix.WebApi.Services
{
    public interface IAiService
    {
        Task<(bool IsSuccess, string ResponseContent, string? ErrorMessage)> PredictRiskAsync(JsonElement payload);
        Task<(bool IsSuccess, string ResponseContent, string? ErrorMessage)> ChatAsync(JsonElement payload, string username);
        IAsyncEnumerable<string> StreamChatAsync(JsonElement payload, string username, CancellationToken cancellationToken);
    }
}
