using Royal_Blueberry_Dictionary.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service.ApiClient
{
    public class BackendApiClient : IBackendApiClient   
    {
        HttpClient httpClient;
        
        public BackendApiClient(ApiSettings apiSettings) 
        {
            httpClient = new HttpClient() 
            {
                BaseAddress = new Uri(apiSettings.BaseUrl),
                Timeout = TimeSpan.FromSeconds(apiSettings.Timeout) 
            };
        }

        public async Task DeleteAsync(string endpoint)
        {
            AttachBearerToken();
            try
            {
                var response = await httpClient.DeleteAsync(endpoint);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in DeleteAsync: {e.Message}");
            }
        }
        public async Task<T> GetAsync<T>(string endpoint)
        {
            AttachBearerToken(); 
            try
            {
                 var response = await httpClient.GetAsync(endpoint);     
                 response.EnsureSuccessStatusCode();
                 var json = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });    

            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in GetAsync: {e.Message}");
                return default(T); 
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            AttachBearerToken();    
            try
            {
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");  

                var response = await httpClient.PostAsync(endpoint, content);  
                response.EnsureSuccessStatusCode();

                var ResponeJson = await response.Content.ReadAsStringAsync();   

                return JsonSerializer.Deserialize<T>(ResponeJson, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                }); 

            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in PostAsync: {e.Message}");
                return default;
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            AttachBearerToken();
            try
            {
                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in PutAsync: {e.Message}");
                return default;
            }
        }
        private void AttachBearerToken()
        {
            var token = TokenManager.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                // Cú pháp chuẩn của JWT là "Bearer chuỗi_token"
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
