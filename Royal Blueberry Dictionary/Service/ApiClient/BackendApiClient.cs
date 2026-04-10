using Royal_Blueberry_Dictionary.Config;
using Royal_Blueberry_Dictionary.Model.Auth;
using Royal_Blueberry_Dictionary.Model.Google;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Royal_Blueberry_Dictionary.Service.ApiClient
{
    public class BackendApiClient : IBackendApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient httpClient;
        private readonly SemaphoreSlim refreshLock = new(1, 1);

        public BackendApiClient(ApiSettings apiSettings)
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiSettings.BaseUrl),
                Timeout = TimeSpan.FromSeconds(apiSettings.Timeout)
            };
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await SendAsync<object>(HttpMethod.Delete, endpoint);
            if (!response.IsSuccess)
            {
                Debug.WriteLine($"Error in DeleteAsync: {response.ErrorMessage}");
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint, bool includeAuthHeader = true)
        {
            var response = await SendAsync<T>(HttpMethod.Get, endpoint, includeAuthHeader: includeAuthHeader);
            if (!response.IsSuccess)
            {
                Debug.WriteLine($"Error in GetAsync: {response.ErrorMessage}");
            }

            return response.Data;
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, bool includeAuthHeader = true)
        {
            var response = await SendAsync<T>(HttpMethod.Post, endpoint, data, includeAuthHeader);
            if (!response.IsSuccess)
            {
                Debug.WriteLine($"Error in PostAsync: {response.ErrorMessage}");
            }

            return response.Data;
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data, bool includeAuthHeader = true)
        {
            var response = await SendAsync<T>(HttpMethod.Put, endpoint, data, includeAuthHeader);
            if (!response.IsSuccess)
            {
                Debug.WriteLine($"Error in PutAsync: {response.ErrorMessage}");
            }

            return response.Data;
        }

        public async Task<ApiResponse<T>> SendAsync<T>(
            HttpMethod method,
            string endpoint,
            object? data = null,
            bool includeAuthHeader = true,
            bool retryOnUnauthorized = true)
        {
            var usedToken = includeAuthHeader ? TokenManager.GetAccessToken() : null;

            try
            {
                using var request = BuildRequest(method, endpoint, data, usedToken);
                using var response = await httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized && includeAuthHeader && retryOnUnauthorized)
                {
                    var refreshed = await TryRefreshTokenAsync(usedToken);
                    if (refreshed)
                    {
                        using var retryRequest = BuildRequest(method, endpoint, data, TokenManager.GetAccessToken());
                        using var retryResponse = await httpClient.SendAsync(retryRequest);
                        return await ReadResponseAsync<T>(retryResponse);
                    }
                }

                return await ReadResponseAsync<T>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in SendAsync({method} {endpoint}): {e.Message}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = e.Message
                };
            }
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string endpoint, object? data, string? bearerToken)
        {
            var request = new HttpRequestMessage(method, endpoint);

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            if (data != null && method != HttpMethod.Get && method != HttpMethod.Delete)
            {
                var json = JsonSerializer.Serialize(data, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return request;
        }

        private async Task<ApiResponse<T>> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            var payload = response.Content == null
                ? string.Empty
                : await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(payload))
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = true,
                        StatusCode = response.StatusCode
                    };
                }

                var data = JsonSerializer.Deserialize<T>(payload, JsonOptions);
                return new ApiResponse<T>
                {
                    IsSuccess = true,
                    StatusCode = response.StatusCode,
                    Data = data
                };
            }

            ApiErrorResponse? error = null;
            if (!string.IsNullOrWhiteSpace(payload))
            {
                try
                {
                    error = JsonSerializer.Deserialize<ApiErrorResponse>(payload, JsonOptions);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to parse API error: {ex.Message}");
                }
            }

            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = response.StatusCode,
                ErrorMessage = error?.Message ?? error?.Error ?? response.ReasonPhrase ?? "Request failed.",
                ValidationErrors = error?.Errors ?? new Dictionary<string, string>()
            };
        }

        private async Task<bool> TryRefreshTokenAsync(string? failedAccessToken)
        {
            await refreshLock.WaitAsync();
            try
            {
                var latestAccessToken = TokenManager.GetAccessToken();
                if (!string.IsNullOrWhiteSpace(failedAccessToken) &&
                    !string.Equals(failedAccessToken, latestAccessToken, StringComparison.Ordinal))
                {
                    return true;
                }

                var refreshToken = TokenManager.GetRefreshToken();
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    TokenManager.ClearTokens();
                    App.UserId = "GUEST";
                    return false;
                }

                using var refreshRequest = BuildRequest(
                    HttpMethod.Post,
                    "auth/refresh-token",
                    new RefreshTokenRequest { RefreshToken = refreshToken },
                    null);

                using var refreshResponse = await httpClient.SendAsync(refreshRequest);
                var refreshResult = await ReadResponseAsync<AuthResponse>(refreshResponse);

                if (!refreshResult.IsSuccess || refreshResult.Data == null || string.IsNullOrWhiteSpace(refreshResult.Data.AccessToken))
                {
                    TokenManager.ClearTokens();
                    App.UserId = "GUEST";
                    return false;
                }

                TokenManager.SaveTokens(
                    refreshResult.Data.AccessToken,
                    string.IsNullOrWhiteSpace(refreshResult.Data.RefreshToken)
                        ? refreshToken
                        : refreshResult.Data.RefreshToken);

                if (!string.IsNullOrWhiteSpace(refreshResult.Data.User?.Id))
                {
                    App.UserId = refreshResult.Data.User.Id;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in TryRefreshTokenAsync: {e.Message}");
                return false;
            }
            finally
            {
                refreshLock.Release();
            }
        }
    }
}
