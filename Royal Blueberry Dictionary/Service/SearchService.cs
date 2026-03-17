using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Database;
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
        private readonly AppDbContext dbContext;
        private readonly Dictionary<string, WordDetail> cache = new();
        private readonly int cachedExpirationDate;

        public SearchService(IBackendApiClient backendApiClient, AppDbContext appDbContext)
        {
            this.backendApiClient = backendApiClient;
            this.dbContext = appDbContext;
            this.cachedExpirationDate = App.serviceProvider.GetRequiredService<Config.ApiSettings>().cachedExpirationDate;
            loadCacheDataFromDB();
        }

        public async Task<WordDetail> searchAWord(string word)
        {
            word = word.ToLower().Trim();

            // 1. Kiểm tra Cache
            if (cache.ContainsKey(word))
            {
                return cache[word];
            }

            // 2. Gọi API nếu không có trong cache hoặc cache quá hạn
            var response = await backendApiClient.GetAsync<WordDetail>($"searching/get-detail/{word}");

            if (response != null)
            {
                await saveToCacheAsync(word, response);
            }

            return response;
        }

        private void loadCacheDataFromDB()
        {
            var expirationDate = DateTime.UtcNow.AddDays(-cachedExpirationDate);

            var validCaches = dbContext.CachedWords
                .Where(cw => cw.CachedAt >= expirationDate)
                .ToList();

            foreach (var cachedWord in validCaches)
            {
                if (!string.IsNullOrEmpty(cachedWord.DataJson))
                {
                    var detail = System.Text.Json.JsonSerializer.Deserialize<WordDetail>(cachedWord.DataJson);
                    cache[cachedWord.Word] = detail;
                }
            }

            var oldCaches = dbContext.CachedWords.Where(cw => cw.CachedAt < expirationDate);
            if (oldCaches.Any())
            {
                dbContext.CachedWords.RemoveRange(oldCaches);
                dbContext.SaveChanges();
            }
        }

        private async Task saveToCacheAsync(string word, WordDetail detail)
        {
            cache[word] = detail;


            var existing = await dbContext.CachedWords.FindAsync(word);
            if (existing != null)
            {
                existing.DataJson = System.Text.Json.JsonSerializer.Serialize(detail);
                existing.CachedAt = DateTime.UtcNow;
            }
            else
            {
                dbContext.CachedWords.Add(new CachedWord
                {
                    Word = word,
                    DataJson = System.Text.Json.JsonSerializer.Serialize(detail),
                    CachedAt = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
