using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Royal_Blueberry_Dictionary.Model.Google
{
    public class GoogleLoginResponse
    {
        public string url {  get; set; } = string.Empty;   
        public string state { get; set; } = string.Empty; 

        public string redirectUri {  get; set; }    = string.Empty;
        public string scope {  get; set; } = string.Empty;
    }
}
