using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Config
{
    public  class ApiSettings
    {
        public string BaseUrl { get; set; }
        public int Timeout { get; set; }
        public int cachedExpirationDate {get; set; }    
    }
}
