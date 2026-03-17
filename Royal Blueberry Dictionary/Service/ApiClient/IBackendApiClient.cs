using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service.ApiClient
{
    public interface IBackendApiClient
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task DeleteAsync(string endpoint);  
    }
}
