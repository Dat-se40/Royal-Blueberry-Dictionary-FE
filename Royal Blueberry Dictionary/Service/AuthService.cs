using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Google;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Royal_Blueberry_Dictionary.Service
{
    public class AuthService
    {
        IBackendApiClient backendApiClient;
        User currentUser; 
        public AuthService(IBackendApiClient backend) 
        {
            backendApiClient = backend; 
        }
        public async Task Login()
        {
            var re = await backendApiClient.GetAsync<GoogleLoginResponse>(@"auth/google/url");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(re.redirectUri + @"/");
            listener.Start();

            Process.Start(new ProcessStartInfo(re.url) { UseShellExecute = true });

            HttpListenerContext context = await listener.GetContextAsync();
            var request = context.Request;

            // Lấy param từ URL
            string code = request.QueryString["code"];
            string state = request.QueryString["state"]; 
            // Trả lời cho trình duyệt một câu "Done!"
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Login successful! You can close this tab.");
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            listener.Stop();
            var login = await backendApiClient.PostAsync<AuthResponse>(@"auth/google", new GoogleLoginRequest() 
            {
                code = code , state = state
            });
            UpdateUser(login); 
        }
        public async Task RefreshToken()
        {
            var auth = await backendApiClient.PostAsync<AuthResponse>(@"auth/refresh_token",TokenManager.GetRefreshToken());
            if (auth != null) UpdateUser(auth);
        }

        private void UpdateUser(AuthResponse auth)
        {
            TokenManager.SaveTokens(auth.AccessToken, auth.RefreshToken);
            currentUser = auth.User;
            App.UserId = currentUser.Id;    
        }
        public User CurrentUser => currentUser; 

    }
}
