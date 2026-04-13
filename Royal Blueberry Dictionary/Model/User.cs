using System.Text.Json.Serialization;

namespace Royal_Blueberry_Dictionary.Model
{
    public class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Name: {Name} | Email: {Email} | Role: {Role}";
        }
    }
}
