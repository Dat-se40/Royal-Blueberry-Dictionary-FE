using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Auth;
using Royal_Blueberry_Dictionary.Model.Google;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Royal_Blueberry_Dictionary.Service
{
    public class AuthService
    {
        private readonly IBackendApiClient backendApiClient;
        private User? currentUser;

        public AuthService(IBackendApiClient backend)
        {
            backendApiClient = backend;
            TokenManager.TokensChanged += OnTokensChanged;
        }

        public event EventHandler? AuthStateChanged;

        public User? CurrentUser => currentUser;

        public bool IsAuthenticated => currentUser != null && TokenManager.HasStoredTokens();

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            var response = await backendApiClient.SendAsync<AuthResponse>(
                HttpMethod.Post,
                "auth/register",
                request,
                includeAuthHeader: false,
                retryOnUnauthorized: false);

            return await CompleteAuthResponseAsync(response);
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var response = await backendApiClient.SendAsync<AuthResponse>(
                HttpMethod.Post,
                "auth/login",
                request,
                includeAuthHeader: false,
                retryOnUnauthorized: false);

            return await CompleteAuthResponseAsync(response);
        }

        public async Task<AuthResult> LoginWithGoogleAsync()
        {
            var urlResponse = await backendApiClient.SendAsync<GoogleLoginResponse>(
                HttpMethod.Get,
                "auth/google/url",
                includeAuthHeader: false,
                retryOnUnauthorized: false);

            if (!urlResponse.IsSuccess || urlResponse.Data == null)
            {
                return ToAuthResult(urlResponse);
            }

            if (!Uri.TryCreate(urlResponse.Data.redirectUri, UriKind.Absolute, out var redirectUri))
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Backend returned an invalid Google redirect URI."
                };
            }

            var listenerPrefix = GetListenerPrefix(redirectUri);
            using var listener = new HttpListener();
            listener.Prefixes.Add(listenerPrefix);

            try
            {
                listener.Start();
                Process.Start(new ProcessStartInfo(urlResponse.Data.url) { UseShellExecute = true });

                var contextTask = listener.GetContextAsync();
                var completedTask = await Task.WhenAny(contextTask, Task.Delay(TimeSpan.FromMinutes(5)));
                if (completedTask != contextTask)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Google sign-in timed out."
                    };
                }

                var context = await contextTask;
                var request = context.Request;
                var code = request.QueryString["code"];
                var state = request.QueryString["state"];
                var error = request.QueryString["error"];

                WriteBrowserResponse(context.Response, string.IsNullOrWhiteSpace(error));

                if (!string.IsNullOrWhiteSpace(error))
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Google sign-in failed: {error}"
                    };
                }

                var loginResponse = await backendApiClient.SendAsync<AuthResponse>(
                    HttpMethod.Post,
                    "auth/google",
                    new GoogleLoginRequest
                    {
                        code = code ?? string.Empty,
                        state = state ?? string.Empty
                    },
                    includeAuthHeader: false,
                    retryOnUnauthorized: false);

                return await CompleteAuthResponseAsync(loginResponse);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Google login error: {ex.Message}");
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                if (listener.IsListening)
                {
                    listener.Stop();
                }
            }
        }

        public async Task<AuthResult> RefreshSessionAsync()
        {
            var refreshToken = TokenManager.GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                ClearSession(clearTokens: false);
                return new AuthResult
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "No refresh token is stored on this device."
                };
            }

            var response = await backendApiClient.SendAsync<AuthResponse>(
                HttpMethod.Post,
                "auth/refresh-token",
                new RefreshTokenRequest { RefreshToken = refreshToken },
                includeAuthHeader: false,
                retryOnUnauthorized: false);

            if (!response.IsSuccess &&
                (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.NotFound))
            {
                ClearSession();
            }

            return await CompleteAuthResponseAsync(response);
        }

        public async Task<AuthResult> LoadCurrentUserAsync()
        {
            if (!TokenManager.HasStoredTokens())
            {
                ClearSession(clearTokens: false);
                return new AuthResult
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessage = "No stored session found."
                };
            }

            var response = await backendApiClient.SendAsync<User>(HttpMethod.Get, "auth/me");
            if (!response.IsSuccess || response.Data == null)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.NotFound)
                {
                    ClearSession();
                }

                return ToAuthResult(response);
            }

            SetCurrentUser(response.Data);
            return new AuthResult
            {
                IsSuccess = true,
                StatusCode = response.StatusCode,
                User = response.Data
            };
        }

        public async Task<bool> TryRestoreSessionAsync()
        {
            if (!TokenManager.HasStoredTokens())
            {
                ClearSession(clearTokens: false);
                return false;
            }

            var result = await LoadCurrentUserAsync();
            return result.IsSuccess;
        }

        public void ContinueAsGuest()
        {
            ClearSession();
        }

        public void Logout()
        {
            ClearSession();
        }

        private async Task<AuthResult> CompleteAuthResponseAsync(ApiResponse<AuthResponse> response)
        {
            if (!response.IsSuccess || response.Data == null)
            {
                return ToAuthResult(response);
            }

            UpdateSession(response.Data);
            if (currentUser != null)
            {
                return new AuthResult
                {
                    IsSuccess = true,
                    StatusCode = response.StatusCode,
                    User = currentUser
                };
            }

            return await LoadCurrentUserAsync();
        }

        private void UpdateSession(AuthResponse auth)
        {
            var existingRefreshToken = TokenManager.GetRefreshToken() ?? string.Empty;
            var refreshToken = string.IsNullOrWhiteSpace(auth.RefreshToken)
                ? existingRefreshToken
                : auth.RefreshToken;

            if (!string.IsNullOrWhiteSpace(auth.AccessToken))
            {
                TokenManager.SaveTokens(auth.AccessToken, refreshToken);
            }

            if (auth.User != null)
            {
                SetCurrentUser(auth.User);
            }
        }

        private void SetCurrentUser(User? user)
        {
            currentUser = user;
            App.UserId = string.IsNullOrWhiteSpace(user?.Id) ? "GUEST" : user.Id;
            RaiseAuthStateChanged();
        }

        private void ClearSession(bool raiseStateChanged = true, bool clearTokens = true)
        {
            var hadSession = currentUser != null ||
                             !string.Equals(App.UserId, "GUEST", StringComparison.Ordinal) ||
                             TokenManager.HasStoredTokens();

            currentUser = null;
            App.UserId = "GUEST";

            if (clearTokens)
            {
                TokenManager.ClearTokens();
            }

            if (raiseStateChanged && hadSession)
            {
                RaiseAuthStateChanged();
            }
        }

        private void OnTokensChanged(object? sender, EventArgs e)
        {
            if (TokenManager.HasStoredTokens())
            {
                return;
            }

            ClearSession(clearTokens: false);
        }

        private void RaiseAuthStateChanged()
        {
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private static string GetListenerPrefix(Uri redirectUri)
        {
            var path = redirectUri.AbsolutePath;
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path += "/";
            }

            return $"{redirectUri.Scheme}://{redirectUri.Host}:{redirectUri.Port}{path}";
        }

        private static void WriteBrowserResponse(HttpListenerResponse response, bool isSuccess)
        {
            var html = isSuccess
                ? "<html><body style='font-family:Segoe UI;padding:24px;'><h2>Login successful</h2><p>You can close this tab and return to the app.</p></body></html>"
                : "<html><body style='font-family:Segoe UI;padding:24px;'><h2>Login failed</h2><p>Return to the app to continue.</p></body></html>";

            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        private static AuthResult ToAuthResult<T>(ApiResponse<T> response)
        {
            return new AuthResult
            {
                IsSuccess = false,
                StatusCode = response.StatusCode,
                ErrorMessage = response.ErrorMessage,
                ValidationErrors = response.ValidationErrors
            };
        }
    }
}
