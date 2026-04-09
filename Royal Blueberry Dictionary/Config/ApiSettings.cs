namespace Royal_Blueberry_Dictionary.Config
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;

        public int Timeout { get; set; }

        public int cachedExpirationDate { get; set; }
    }
}
