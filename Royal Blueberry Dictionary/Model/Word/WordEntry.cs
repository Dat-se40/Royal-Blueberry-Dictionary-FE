using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public partial class WordEntry : ObservableObject
    {
        public string Word { get; set; }
        [JsonPropertyName("phonetic")]
        public string Phonetic { get; set; } = string.Empty;    
        public int MeaningIndex { get; set; }
        [JsonPropertyName("partOfSpeech")]
        public string PartOfSpeech { get; set; } = string.Empty ;
        [JsonPropertyName("definition")]
        public string Definition { get; set; }
        [JsonPropertyName("example")]
        public string Example { get; set; } = string.Empty; 
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public List<string> TagIdsJson { get; set; } = new List<string>();
        public string Note { get; set; } = string.Empty;    
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsDirty { get; set; } = false; // đã được sync với server chưa  
        [ObservableProperty]
        public bool _isFavorited;
        override public string ToString()
        {
            return $"{Word} ({PartOfSpeech}): {Definition}";
        }
    }
}
