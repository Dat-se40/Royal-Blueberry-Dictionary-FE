    using Microsoft.Extensions.DependencyInjection;
    using Royal_Blueberry_Dictionary.Database;
    using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using Royal_Blueberry_Dictionary.Service.ApiClient;
    using System;
    using System.Collections.Generic;
    using System.IO;
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
            private readonly Dictionary<string, DateTime > timeLogs = new();    
            private readonly int cachedExpirationDate;
            private HashSet<string> _availableWords = new();
            public SearchService(IBackendApiClient backendApiClient, AppDbContext appDbContext)
            {
                this.backendApiClient = backendApiClient;
                this.dbContext = appDbContext;
                this.cachedExpirationDate = App.serviceProvider.GetRequiredService<Config.ApiSettings>().cachedExpirationDate;
                loadCacheDataFromDB();
                loadAvailableWordList();
            }
            #region Main Search Logic
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

                if (response != null && IsValidWordDetail(response))
                {
                    await saveToCacheAsync(word, response);
                }
                return response;
            }

            private void loadCacheDataFromDB()
            {
                var expirationDate = DateTime.UtcNow.AddDays(-cachedExpirationDate);

                var validCaches = dbContext.CachedWords
                    .Where((cw) => cw.CachedAt >= expirationDate).ToList();

                foreach (var cachedWord in validCaches)
                {
                    timeLogs[cachedWord.Word] = cachedWord.CachedAt; 
                    if (!string.IsNullOrEmpty(cachedWord.DataJson))
                    {
                        var detail = System.Text.Json.JsonSerializer.Deserialize<WordDetail>(cachedWord.DataJson);
                        cache[cachedWord.Word] = detail;
                    }else timeLogs.Remove(cachedWord.Word);
                
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
                    timeLogs[word] = DateTime.UtcNow;
                }
                await dbContext.SaveChangesAsync();
                _availableWords.Add(word);
            }
            #endregion
            #region Levenshtein Implementation

            public async Task<List<string>> GetSuggestionsAsync(string input, int maxSuggestions = 5)
            {
                if (string.IsNullOrWhiteSpace(input)) return new List<string>();

                input = input.ToLower().Trim();

                // Thực hiện tính toán LD trên background thread để tránh treo UI
                return await Task.Run(() =>
                {
                    return _availableWords
                        .Select(word => new { Word = word, Distance = CalculateLevenshteinDistance(input, word) })
                        .Where(x => x.Distance <= 3) // Chỉ lấy các từ có sai số tối đa 3 ký tự
                        .OrderBy(x => x.Distance)
                        .ThenBy(x => x.Word.Length)
                        .Take(maxSuggestions)
                        .Select(x => x.Word)
                        .ToList();
                });
            }

            private int CalculateLevenshteinDistance(string source, string target)
            {
                if (string.IsNullOrEmpty(source)) return target.Length;
                if (string.IsNullOrEmpty(target)) return source.Length;

                int n = source.Length;
                int m = target.Length;
                int[,] distance = new int[n + 1, m + 1];

                for (int i = 0; i <= n; distance[i, 0] = i++) ;
                for (int j = 0; j <= m; distance[0, j] = j++) ;

                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                        distance[i, j] = Math.Min(
                            Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                            distance[i - 1, j - 1] + cost);
                    }
                }
                return distance[n, m];
            }

            #endregion
            private void loadAvailableWordList()
            {
           
               var fileWords = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,@"Database\AvailableWordList.txt"));
               var dbWords = cache.Keys;
               foreach (var w in dbWords) _availableWords.Add(w.ToLower());
               _availableWords.UnionWith(fileWords);
               Console.WriteLine($"Loaded {_availableWords.Count} available words for suggestions.");   
            }
            //<summary>
            // Dành cho mục đích debug, có thể dùng để xem nhanh cache hiện tại đang có những từ nào.   
            //</summary>
            public IEnumerable<WordDetail> getHistroyCacheToday() 
            {
                // Backward-compatible API: return all cached items sorted by last access (desc).
                // The old name says "Today", but the app's History page expects a full timeline.
                foreach (var key in timeLogs.OrderByDescending(kv => kv.Value).Select(kv => kv.Key))
                {
                    if (!cache.TryGetValue(key, out var value)) continue;
                    if (!IsValidWordDetail(value)) continue;
                    yield return value;
                }
            } 
            public async Task RemoveWordInCacheAsync(string word) 
            {
                if (string.IsNullOrWhiteSpace(word)) return;
                word = word.ToLower().Trim();

                cache.Remove(word);
                timeLogs.Remove(word);

                var target = dbContext.CachedWords.FirstOrDefault(entity => entity.Word == word);
                if (target != null)
                {
                    dbContext.CachedWords.Remove(target);
                    await dbContext.SaveChangesAsync();
                }
        }

        public async Task ClearHistoryAsync()
        {
            cache.Clear();
            timeLogs.Clear();
            dbContext.CachedWords.RemoveRange(dbContext.CachedWords);
            await dbContext.SaveChangesAsync();
        }
        // Cải thiện hàm này với các từ bị trỗng
            public bool IsValidWordDetail(WordDetail wordDetail) 
            {
                return wordDetail.Word != string.Empty &&wordDetail.Meanings != null && wordDetail.Meanings.Count != 0;
            }
            
        
        }
    }
