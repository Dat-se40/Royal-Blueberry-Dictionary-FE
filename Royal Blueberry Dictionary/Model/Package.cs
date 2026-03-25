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

        public string name; 

        public string category;

        public string level; 

        public int totalWords;

        public string description;

        [JsonPropertyName("updateAt")]
        public string updateAt;


        public bool isDirty; // Đánh dấu để sync lên Server sau 

        public override string ToString()
        {
            return $"{name} + ({category}, {level}) - {totalWords} words";
        }
    }
}
