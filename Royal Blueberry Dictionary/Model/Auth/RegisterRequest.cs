namespace Royal_Blueberry_Dictionary.Model.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? DisplayName { get; set; }
    }
}
