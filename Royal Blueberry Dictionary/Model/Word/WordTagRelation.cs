using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model.Word
{
    public class WordTagRelation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Word { get; set; }        // Tên từ (Key để fetch lại)
        public int MeaningIndex { get; set; }   // Nghĩa cụ thể (Key để fetch lại)
        public string TagId { get; set; }       // Liên kết tới Tag
        public bool IsFavourite { get; set; } = false;

        public string Note { get; set; } = string.Empty; 
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
        public bool IsDirty { get; set; } = true; // Dùng để đồng bộ
    }
}
