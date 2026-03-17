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

        public async Task<WordEntry> searchAWord(string word) 
        {
            var reponse = await backendApiClient.GetAsync<WordEntry>($"search/get-detail/{word}");
            return reponse;    
        }
    }
}
