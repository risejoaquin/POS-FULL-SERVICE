using System.Text.Json.Serialization;

namespace PosServer.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;
    }
}