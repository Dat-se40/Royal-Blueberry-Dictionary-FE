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
        private const string AccessTokenPath = "access_token.bin";
        private const string RefreshTokenPath = "refresh_token.bin";

        public static void SaveTokens(string accessToken, string refreshToken)
        {
            File.WriteAllText(AccessTokenPath, accessToken);
            File.WriteAllText(RefreshTokenPath, refreshToken);
        }

        public static void ClearToken()
        {
            if (File.Exists(AccessTokenPath)) File.Delete(AccessTokenPath);
            if (File.Exists(RefreshTokenPath)) File.Delete(RefreshTokenPath);
        }

        public static string GetAccessToken()
            => File.Exists(AccessTokenPath) ? File.ReadAllText(AccessTokenPath) : null;

        public static string GetRefreshToken()
            => File.Exists(RefreshTokenPath) ? File.ReadAllText(RefreshTokenPath) : null;
    }
}
