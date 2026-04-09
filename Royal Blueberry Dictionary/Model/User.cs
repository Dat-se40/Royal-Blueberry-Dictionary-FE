using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("displayName")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; } // Sửa lại Property Name cho chuẩn C#

        [JsonPropertyName("role")]
        public string Role { get; set; }

        public override string ToString()
        {
            return $"Name: {Name} | Email: {Email} | Role: {Role}";
        }
    }
}
