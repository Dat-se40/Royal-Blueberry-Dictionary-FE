using Royal_Blueberry_Dictionary.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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

        public Task DeleteAsync(string endpoint)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
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

        public Task<T> PutAsync<T>(string endpoint, object data)
        {
            throw new NotImplementedException();
        }
    }
}
