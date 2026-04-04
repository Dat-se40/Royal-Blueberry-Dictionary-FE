using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model
{
    public class CachedWord
    {
        public string Word { get; set; }
        public string DataJson { get; set; }
        public DateTime CachedAt { get; set; } = DateTime.UtcNow;
    }
}
