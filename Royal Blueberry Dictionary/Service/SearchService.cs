using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service
{
    public class SearchService
    {
        private readonly IBackendApiClient backendApiClient;

        public SearchService(IBackendApiClient backendApiClient)
        {
            this.backendApiClient = backendApiClient;
        }   

        public async Task<WordDetail> searchAWord(string word) 
        {
            var response = await backendApiClient.GetAsync<WordDetail>($"searching/get-detail/{word}");
            return response;
        }
    }
}
