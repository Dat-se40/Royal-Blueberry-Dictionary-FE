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
            private readonly HashSet<string> _availableWords = new(StringComparer.Ordinal);
            private List<string> _sortedWords = new();
            private readonly Dictionary<char, List<string>> _wordsByFirstLetter = new();
            private HashSet<string> _recentSearchWords = new(StringComparer.Ordinal);
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
                AddWordToIndex(word);
                _recentSearchWords.Add(word);
            }
            #endregion
            #region Suggestion Logic

            public async Task<List<string>> GetSuggestionsAsync(string input, int maxSuggestions = 8)
            {
                if (string.IsNullOrWhiteSpace(input)) return new List<string>();

                input = input.ToLower().Trim();
                if (input.Length < 1) return new List<string>();

                return await Task.Run(() => BuildSuggestions(input, maxSuggestions));
            }

            private List<string> BuildSuggestions(string input, int maxSuggestions)
            {
                var ranked = new Dictionary<string, int>(StringComparer.Ordinal);

                void TryAdd(string word, int score)
                {
                    if (!ranked.TryGetValue(word, out var existing) || score < existing)
                    {
                        ranked[word] = score;
                    }
                }

                // Ưu tiên 1: lịch sử tìm kiếm gần đây khớp prefix
                foreach (var word in _recentSearchWords)
                {
                    if (word.StartsWith(input, StringComparison.Ordinal))
                    {
                        TryAdd(word, ScorePrefixMatch(input, word, isRecent: true));
                    }
                }

                // Ưu tiên 2: prefix match từ danh sách từ (binary search trên list đã sort)
                foreach (var word in GetPrefixMatches(input))
                {
                    TryAdd(word, ScorePrefixMatch(input, word, isRecent: _recentSearchWords.Contains(word)));
                }

                // Ưu tiên 3: chứa chuỗi con (khi gõ sai vị trí đầu)
                if (ranked.Count < maxSuggestions && input.Length >= 3)
                {
                    foreach (var word in GetContainsMatches(input))
                    {
                        TryAdd(word, ScoreContainsMatch(input, word, isRecent: _recentSearchWords.Contains(word)));
                    }
                }

                // Ưu tiên 4: fuzzy Levenshtein (chỉ khi chưa đủ gợi ý)
                if (ranked.Count < maxSuggestions)
                {
                    int maxDistance = GetMaxLevenshteinDistance(input.Length);
                    foreach (var word in GetFuzzyCandidates(input))
                    {
                        int distance = CalculateLevenshteinDistance(input, word);
                        if (distance <= maxDistance)
                        {
                            TryAdd(word, ScoreFuzzyMatch(input, word, distance, isRecent: _recentSearchWords.Contains(word)));
                        }
                    }
                }

                return ranked
                    .OrderBy(x => x.Value)
                    .ThenBy(x => x.Key.Length)
                    .ThenBy(x => x.Key, StringComparer.Ordinal)
                    .Take(maxSuggestions)
                    .Select(x => x.Key)
                    .ToList();
            }

            private IEnumerable<string> GetPrefixMatches(string input)
            {
                int start = FindPrefixStartIndex(input);
                for (int i = start; i < _sortedWords.Count; i++)
                {
                    var word = _sortedWords[i];
                    if (!word.StartsWith(input, StringComparison.Ordinal))
                    {
                        break;
                    }
                    yield return word;
                    if (i - start > 200) yield break;
                }
            }

            private int FindPrefixStartIndex(string input)
            {
                int low = 0;
                int high = _sortedWords.Count;
                while (low < high)
                {
                    int mid = (low + high) / 2;
                    if (string.Compare(_sortedWords[mid], input, StringComparison.Ordinal) < 0)
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        high = mid;
                    }
                }
                return low;
            }

            private IEnumerable<string> GetContainsMatches(string input)
            {
                if (input.Length == 0) yield break;

                char first = input[0];
                if (!_wordsByFirstLetter.TryGetValue(first, out var bucket)) yield break;

                int count = 0;
                foreach (var word in bucket)
                {
                    if (word.Contains(input, StringComparison.Ordinal) && !word.StartsWith(input, StringComparison.Ordinal))
                    {
                        yield return word;
                        if (++count >= 50) yield break;
                    }
                }
            }

            private IEnumerable<string> GetFuzzyCandidates(string input)
            {
                if (input.Length == 0) yield break;

                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (char c in GetNearbyLetters(input[0]))
                {
                    if (!_wordsByFirstLetter.TryGetValue(c, out var bucket)) continue;

                    foreach (var word in bucket)
                    {
                        if (seen.Add(word))
                        {
                            yield return word;
                        }
                    }
                }
            }

            private static IEnumerable<char> GetNearbyLetters(char c)
            {
                yield return c;
                if (c > 'a') yield return (char)(c - 1);
                if (c < 'z') yield return (char)(c + 1);
            }

            private static int GetMaxLevenshteinDistance(int inputLength) =>
                inputLength switch
                {
                    <= 2 => 1,
                    <= 4 => 2,
                    <= 7 => 3,
                    _ => 4
                };

            private static int ScorePrefixMatch(string input, string word, bool isRecent)
            {
                int score = word.Length - input.Length;
                if (isRecent) score -= 20;
                return score;
            }

            private static int ScoreContainsMatch(string input, string word, bool isRecent)
            {
                int score = 100 + word.IndexOf(input, StringComparison.Ordinal);
                if (isRecent) score -= 15;
                return score;
            }

            private static int ScoreFuzzyMatch(string input, string word, int distance, bool isRecent)
            {
                int score = 300 + (distance * 20) + Math.Abs(word.Length - input.Length);
                if (isRecent) score -= 15;
                return score;
            }

            private int CalculateLevenshteinDistance(string source, string target)
            {
                if (string.IsNullOrEmpty(source)) return target.Length;
                if (string.IsNullOrEmpty(target)) return source.Length;

                int n = source.Length;
                int m = target.Length;
                int[] prev = new int[m + 1];
                int[] curr = new int[m + 1];

                for (int j = 0; j <= m; j++) prev[j] = j;

                for (int i = 1; i <= n; i++)
                {
                    curr[0] = i;
                    for (int j = 1; j <= m; j++)
                    {
                        int cost = target[j - 1] == source[i - 1] ? 0 : 1;
                        curr[j] = Math.Min(
                            Math.Min(curr[j - 1] + 1, prev[j] + 1),
                            prev[j - 1] + cost);
                    }
                    (prev, curr) = (curr, prev);
                }
                return prev[m];
            }

            #endregion
            private void loadAvailableWordList()
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Database\AvailableWordList.txt");
                if (File.Exists(filePath))
                {
                    foreach (var line in File.ReadAllLines(filePath))
                    {
                        var word = line.Trim().ToLower();
                        if (word.Length >= 1 && word.All(char.IsLetter))
                        {
                            _availableWords.Add(word);
                        }
                    }
                }

                foreach (var w in cache.Keys)
                {
                    _availableWords.Add(w.ToLower());
                }

                RebuildWordIndexes();
                Console.WriteLine($"Loaded {_availableWords.Count} available words for suggestions.");
            }

            private void AddWordToIndex(string word)
            {
                if (!_availableWords.Add(word)) return;

                int insertIndex = _sortedWords.BinarySearch(word, StringComparer.Ordinal);
                if (insertIndex < 0)
                {
                    _sortedWords.Insert(~insertIndex, word);
                }

                char first = word[0];
                if (!_wordsByFirstLetter.TryGetValue(first, out var bucket))
                {
                    bucket = new List<string>();
                    _wordsByFirstLetter[first] = bucket;
                }
                bucket.Add(word);
            }

            private void RebuildWordIndexes()
            {
                _sortedWords = _availableWords.OrderBy(w => w, StringComparer.Ordinal).ToList();
                _wordsByFirstLetter.Clear();

                foreach (var word in _sortedWords)
                {
                    char first = word[0];
                    if (!_wordsByFirstLetter.TryGetValue(first, out var bucket))
                    {
                        bucket = new List<string>();
                        _wordsByFirstLetter[first] = bucket;
                    }
                    bucket.Add(word);
                }

                _recentSearchWords = timeLogs
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => kv.Key.ToLower())
                    .Where(_availableWords.Contains)
                    .Take(50)
                    .ToHashSet(StringComparer.Ordinal);
            }
        /// <summary>
        /// Lấy ra lịch sử tìm kiếm chỉ trong ngày hôm nay (UTC).
        /// </summary>
        public IEnumerable<WordDetail> getHistroyCacheToday()
        {
            var today = DateTime.UtcNow.Date; 
            var sortedLogs = timeLogs.OrderByDescending(kv => kv.Value).ToList();

            foreach (var kv in sortedLogs)
            {
                if (kv.Value.Date == today)
                {
                    if (cache.TryGetValue(kv.Key, out var value) && IsValidWordDetail(value))
                    {
                        yield return value;
                    }
                }
            }
        }
        public async Task RemoveWordInCacheAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;
            word = word.ToLower().Trim(); 
            cache.Remove(word);
            timeLogs.Remove(word);
            var targets = dbContext.CachedWords
                                   .Where(entity => entity.Word.ToLower() == word)
                                   .ToList();
            if (targets.Any())
            {
                dbContext.CachedWords.RemoveRange(targets);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearHistoryAsync()
        {
            cache.Clear();
            timeLogs.Clear();

            var allCaches = dbContext.CachedWords.ToList(); 
            if (allCaches.Any())
            {
                dbContext.CachedWords.RemoveRange(allCaches);
                await dbContext.SaveChangesAsync();
            }
        }
        // Cải thiện hàm này với các từ bị trỗng
        public bool IsValidWordDetail(WordDetail wordDetail) 
            {
                return wordDetail.Word != string.Empty &&wordDetail.Meanings != null && wordDetail.Meanings.Count != 0;
            }
            
        
        }
    }
