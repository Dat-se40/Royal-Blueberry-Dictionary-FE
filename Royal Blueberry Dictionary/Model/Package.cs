using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public class Package
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonPropertyName("name")]
        public string name { get; set; }    
        [JsonPropertyName("category")] 
        public string category { get; set; }    
        [JsonPropertyName("level")]
        public string level { get; set; }
        [JsonPropertyName("totalWords")]
        public int totalWords { get; set; } 
        [JsonPropertyName("description")]
        public string description { get; set; } 

        [JsonPropertyName("updateAt")]
        public string updateAt;


        public bool isDirty; // Đánh dấu để sync lên Server sau 

        public override string ToString()
        {
            return $"{name} + ({category}, {level}) - {totalWords} words";
        }
    }
}
