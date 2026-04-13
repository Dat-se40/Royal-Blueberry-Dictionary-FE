using System.IO;

namespace Royal_Blueberry_Dictionary.Service
{
    public class TokenManager
    {
        private const string AccessTokenPath = "access_token.bin";
        private const string RefreshTokenPath = "refresh_token.bin";

        public static event EventHandler? TokensChanged;

        public static void SaveTokens(string accessToken, string refreshToken)
        {
            File.WriteAllText(AccessTokenPath, accessToken);
            File.WriteAllText(RefreshTokenPath, refreshToken);
            RaiseTokensChanged();
        }

        public static string? GetAccessToken() =>
            File.Exists(AccessTokenPath) ? File.ReadAllText(AccessTokenPath) : null;

        public static string? GetRefreshToken() =>
            File.Exists(RefreshTokenPath) ? File.ReadAllText(RefreshTokenPath) : null;

        public static bool HasStoredTokens() =>
            !string.IsNullOrWhiteSpace(GetAccessToken()) &&
            !string.IsNullOrWhiteSpace(GetRefreshToken());

        public static void ClearTokens()
        {
            var hasChanges = false;

            if (File.Exists(AccessTokenPath))
            {
                File.Delete(AccessTokenPath);
                hasChanges = true;
            }

            if (File.Exists(RefreshTokenPath))
            {
                File.Delete(RefreshTokenPath);
                hasChanges = true;
            }

            if (hasChanges)
            {
                RaiseTokensChanged();
            }
        }

        private static void RaiseTokensChanged()
        {
            TokensChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
