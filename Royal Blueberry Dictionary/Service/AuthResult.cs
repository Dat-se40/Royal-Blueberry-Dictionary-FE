using System.Net;
using Royal_Blueberry_Dictionary.Model;

namespace Royal_Blueberry_Dictionary.Service
{
    public class AuthResult
    {
        public bool IsSuccess { get; init; }

        public HttpStatusCode StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        public Dictionary<string, string> ValidationErrors { get; init; } = new();

        public User? User { get; init; }
    }
}
