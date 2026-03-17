using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public class Tag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
        public bool IsDirty { get; set; } = false;
    }
}
