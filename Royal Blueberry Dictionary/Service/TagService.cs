using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Royal_Blueberry_Dictionary.Service
{
    public class TagService
    {
        private readonly Database.AppDbContext dbContext;
        private readonly IBackendApiClient backendApiClient;
        private readonly IWordEntryRepository wordEntryRepository;
        public TagService(Database.AppDbContext dbContext, IBackendApiClient backendApiClient, IWordEntryRepository wordEntryRepository)
        {
            this.dbContext = dbContext;
            this.backendApiClient = backendApiClient;
            this.wordEntryRepository = wordEntryRepository;
        }
        public async Task<List<Model.Tag>> getAllTags(string userId)
        {
            var tags = await dbContext.Tags.Where(t => t.UserId == userId).ToListAsync();
            return tags;
        }
        public async Task<List<Tag>> getTagsByUserId(string userId) 
        {
            return await backendApiClient.GetAsync<List<Tag>>($"tags/{userId}");
        }

    }
}
