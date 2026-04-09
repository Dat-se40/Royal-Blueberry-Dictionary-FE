using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        // Tạo sẵn 2 class nhỏ để hứng dữ liệu
        public class LoginRequest { public string email { get; set; } public string password { get; set; } }
        public class AuthResponse { public string token { get; set; } /* có thể có refreshToken, userInfo... */ }

        public AuthService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080/api/auth/") };
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var request = new LoginRequest { email = email, password = password };

            // Gọi API Login của Backend
            var response = await _httpClient.PostAsJsonAsync("login", request);

            if (response.IsSuccessStatusCode)
            {
                var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();

                // LẤY ĐƯỢC TOKEN -> CẤT ĐI (Xem Bước 2)
                TokenManager.SaveToken(authData.token);
                return true;
            }
            return false;
        }

        public void Logout()
        {
            TokenManager.ClearToken();
        }
    }
}
