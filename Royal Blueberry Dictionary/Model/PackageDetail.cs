using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public class PackageDetail
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString(); 

        [JsonPropertyName("packageId")]
        public string PackageId { get; set; }

        public int TotalWords => Words?.Count ?? 0;

        [JsonPropertyName("words")] 
        public List<WordEntry> Words { get; set; } = new List<WordEntry>();
    }
}
