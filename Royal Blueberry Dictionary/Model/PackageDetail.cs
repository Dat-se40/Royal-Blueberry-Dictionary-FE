using System;
using System.Collections.Generic;
using System.Configuration;
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
        [JsonPropertyName("totalWords")]
        public int TotalWords { get; set; }

        [JsonPropertyName("words")] 
        public List<WordEntry> Words { get; set; } = new List<WordEntry>();

        override public string ToString()
        {
            string wordList = Words != null ? string.Join(", ", Words.Select(w => w.Word)) : "No words";
            return $"PackageDetail for PackageId: {PackageId} with {TotalWords} words : {Words}";
        }   

    }
}
