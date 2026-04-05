using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Royal_Blueberry_Dictionary.Model
{
    public class WordDetail
    {
        public string Word { get; set; }    
        public string Phonetic { get; set; }
        public string AudioUs { get; set; }
        public string AudioUk { get; set; }
        public string ImageUrl { get; set; }
        public List<Meaning> Meanings { get; set; } = new List<Meaning>();
    }

    public class Meaning
    {
        public int MeaningIndex { get; set; }
        public string PartOfSpeech { get; set; }
        public List<Definition> Definitions { get; set; } = new List<Definition>();
        public List<string> Synonyms { get; set; } = new List<string>();
        public List<string> Antonyms { get; set; } = new List<string>();
    }

    public class Definition
    {
        public int DefinitionIndex { get; set; }
        [JsonPropertyName("definition")]
        public string Text { get; set; } // Map với 'definition' trong JSON BE
        public string Example { get; set; }
        public List<string> Synonyms { get; set; } = new List<string>();
        public List<string> Antonyms { get; set; } = new List<string>();
    }
}