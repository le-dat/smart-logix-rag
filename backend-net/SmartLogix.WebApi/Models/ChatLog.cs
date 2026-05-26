using System;

namespace SmartLogix.WebApi.Models
{
    public class ChatLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "anonymous";
        public string Prompt { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string LLMProvider { get; set; } = "Claude"; // Claude, GPT, Gemini
        public int? Rating { get; set; } // Nullable rating (e.g. 1-5 stars)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
