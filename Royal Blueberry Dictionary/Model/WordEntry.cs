using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public class WordEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Word { get; set; }
        public int MeaningIndex { get; set; }
        public string PartOfSpeech { get; set; }
        public string Definition { get; set; }
        public string Example { get; set; }
        public string TagIdsJson { get; set; }
        public bool IsFavorited { get; set; } = false;
        public string Note { get; set; }
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsDirty { get; set; } = false; // đã được sync với server chưa  
    }
}
