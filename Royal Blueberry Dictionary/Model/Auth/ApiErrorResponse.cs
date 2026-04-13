using System.Text.Json.Serialization;

namespace Royal_Blueberry_Dictionary.Model.Auth
{
    public class ApiErrorResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, string>? Errors { get; set; }
    }
}
