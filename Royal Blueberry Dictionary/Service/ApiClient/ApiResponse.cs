using System.Net;

namespace Royal_Blueberry_Dictionary.Service.ApiClient
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; init; }

        public HttpStatusCode StatusCode { get; init; }

        public T? Data { get; init; }

        public string? ErrorMessage { get; init; }

        public Dictionary<string, string> ValidationErrors { get; init; } = new();
    }
}
