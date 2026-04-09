using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service
{
    public class TokenManager
    {
        public static void SaveTokens(string accessToken, string refreshToken)
        {
            File.WriteAllText("access_token.bin", accessToken);
            File.WriteAllText("refresh_token.bin", refreshToken); 
        }
        public static string GetAccessToken() => File.Exists("access_token.bin") ? File.ReadAllText("access_token.bin") : null;
        public static string GetRefreshToken() => File.Exists("refresh_token.bin") ? File.ReadAllText("refresh_token.bin") : null;
    }
}
