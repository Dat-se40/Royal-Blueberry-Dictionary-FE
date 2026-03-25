using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public class WordEntry
    {
        public string Word { get; set; }
        [JsonPropertyName("phonetic")]
        public string Phonetic { get; set; }
        public int MeaningIndex { get; set; }
        [JsonPropertyName("partOfSpeech")]
        public string PartOfSpeech { get; set; }
        [JsonPropertyName("definition")]
        public string Definition { get; set; }
        [JsonPropertyName("example")]
        public string Example { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string TagIdsJson { get; set; }
        public bool IsFavorited { get; set; } = false;
        public string Note { get; set; }
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsDirty { get; set; } = false; // đã được sync với server chưa  

        public WordEntry MapWordDetailToWordEntry(WordDetail detail, int meaningIdx, int defIdx)
        {
            var meaning = detail.Meanings[meaningIdx];
            var definition = meaning.Definitions[defIdx];

            return new WordEntry
            {
                Word = detail.Word,
                Phonetic = detail.Phonetic,
                PartOfSpeech = meaning.PartOfSpeech,
                Definition = definition.Text,
                Example = definition.Example,
                MeaningIndex = meaningIdx,
                LastModifiedAt = DateTime.UtcNow,
                IsDirty = true // Đánh dấu để sync lên Server sau
            };
        }
    }
}
