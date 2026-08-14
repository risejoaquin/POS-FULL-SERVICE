with open('PosServer/Models/ErrorResponse.cs', 'r') as f:
    c = f.read()

new_code = """using System.Text.Json.Serialization;

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
}"""

with open('PosServer/Models/ErrorResponse.cs', 'w') as f:
    f.write(new_code)
