using System.Net.Http;

namespace Royal_Blueberry_Dictionary.Service.ApiClient
{
    public interface IBackendApiClient
    {
        Task<ApiResponse<T>> SendAsync<T>(
            HttpMethod method,
            string endpoint,
            object? data = null,
            bool includeAuthHeader = true,
            bool retryOnUnauthorized = true);

        Task<T?> GetAsync<T>(string endpoint, bool includeAuthHeader = true);

        Task<T?> PostAsync<T>(string endpoint, object data, bool includeAuthHeader = true);

        Task<T?> PutAsync<T>(string endpoint, object data, bool includeAuthHeader = true);

        Task DeleteAsync(string endpoint);
    }
}
