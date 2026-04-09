using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Model.Google
{
    public class GoogleLoginRequest
    {
        public string code {  get; set; } = string.Empty;
        public string state { get; set; } =  string.Empty ; 
    }
}
