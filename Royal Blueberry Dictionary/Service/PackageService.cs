using System.Windows.Media;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Word;
using Royal_Blueberry_Dictionary.Repository.Interface;
using Royal_Blueberry_Dictionary.Service.ApiClient;
namespace Royal_Blueberry_Dictionary.Service
{
    /// <summary>
    /// Package catalog + chi tiết từ API (BE: <c>/api/packages</c>, <c>/api/packages/details/&#123;id&#125;</c>).
    /// Tải offline = lưu <see cref="PackageDetail"/> qua <see cref="OfflinePackageCacheService"/>.
    /// </summary>
    public class PackageService
    {
        private readonly IBackendApiClient _api;
        private readonly OfflinePackageCacheService _cache;
        private readonly IWordEntryRepository _wordRepo;
        private readonly ITagRepository _tagRepo;
        private readonly TagService _tagService;

        public PackageService(
            IBackendApiClient api,
            OfflinePackageCacheService cache,
            IWordEntryRepository wordRepo,
            ITagRepository tagRepo,
            TagService tagService)
        {
            _api = api;
            _cache = cache;
            _wordRepo = wordRepo;
            _tagRepo = tagRepo;
            _tagService = tagService;
        }

        public async Task<List<Package>> getAllPackages()
        {
            var list = await _api.GetAsync<List<Package>>(@"packages");
            return list ?? new List<Package>();
        }

        public async Task<PackageDetail?> getDetailByPackageId(string id)
        {
            return await _api.GetAsync<PackageDetail>($@"packages/details/{id}");
        }

        /// <summary>API trước; nếu lỗi thì đọc cache.</summary>
        public async Task<PackageDetail?> GetPackageDetailPreferNetworkAsync(string packageId, CancellationToken ct = default)
        {
            var fromApi = await _api.GetAsync<PackageDetail>($@"packages/details/{packageId}");
            if (fromApi != null)
                return fromApi;
            return await _cache.TryLoadAsync(packageId, ct).ConfigureAwait(false);
        }

        public Task<PackageDetail?> GetCachedDetailAsync(string packageId, CancellationToken ct = default) =>
            _cache.TryLoadAsync(packageId, ct);

        public bool IsOfflineCached(string packageId) => _cache.IsCached(packageId);

        public DateTime? GetOfflineCachedAtUtc(string packageId) => _cache.GetCachedWriteTimeUtc(packageId);

        public long? GetOfflineCachedSizeBytes(string packageId) => _cache.GetCachedSizeBytes(packageId);

        public async Task<bool> DownloadForOfflineAsync(string packageId, CancellationToken ct = default)
        {
            var detail = await _api.GetAsync<PackageDetail>($@"packages/details/{packageId}");
            if (detail == null)
                return false;
            await _cache.SaveAsync(packageId, detail, ct).ConfigureAwait(false);
            return true;
        }

        public Task RemoveOfflineCacheAsync(string packageId) => _cache.DeleteAsync(packageId);

        /// <summary>Import vào My Words: một tag <c>📦 tên package</c> + quan hệ từ–tag.</summary>
        public async Task<int> ImportPackageToMyWordsAsync(Package package, PackageDetail detail, string userId, CancellationToken ct = default)
        {
            if (detail.Words == null || detail.Words.Count == 0)
                return 0;

            var tagName = $"📦 {package.name}";
            var tags = await _tagRepo.GetAllTagsAsync();
            var tag = tags.FirstOrDefault(t => string.Equals(t.Name, tagName, StringComparison.Ordinal));
            if (tag == null)
            {
                tag = new Tag { Name = tagName, Icon = "📦", Color = "#2D4ACC" };
                await _tagRepo.AddTagAsync(tag);
                await _tagRepo.SaveChangesAsync();
            }

            var uid = string.IsNullOrEmpty(userId) ? "GUEST" : userId;
            var added = 0;

            foreach (var w in detail.Words)
            {
                ct.ThrowIfCancellationRequested();
                var word = w.Word?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(word))
                    continue;

                var meaningIndex = w.MeaningIndex;

                var existing = await _wordRepo.GetByWordAndMeaningAsync(uid, word, meaningIndex);
                if (existing == null)
                {
                    var entry = new WordEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        Word = word,
                        Phonetic = w.Phonetic ?? string.Empty,
                        MeaningIndex = meaningIndex,
                        PartOfSpeech = w.PartOfSpeech ?? string.Empty,
                        Definition = w.Definition ?? string.Empty,
                        Example = w.Example ?? string.Empty,
                        Note = string.Empty,
                        UserId = uid,
                        TagIdsJson = new List<string>(),
                        LastModifiedAt = DateTime.UtcNow,
                        IsDirty = true
                    };
                    await _wordRepo.AddAsync(entry);
                    added++;
                }

                await _tagService.LinkWordToTagAsync(uid, word, meaningIndex, tag.Id, false, string.Empty);
            }

            await _tagRepo.SaveChangesAsync();
            return added;
        }

        public static Brush CategoryBrush(string? category)
        {
            var c = category?.ToLowerInvariant() ?? string.Empty;
            var color = c.Contains("ielts") ? Color.FromRgb(0x0E, 0x74, 0x9B)
                : c.Contains("toeic") || c.Contains("business") ? Color.FromRgb(0x7C, 0x3A, 0xED)
                : c.Contains("daily") || c.Contains("conversation") ? Color.FromRgb(0x05, 0x96, 0x69)
                : Color.FromRgb(0x2D, 0x4A, 0xCC);
            return new SolidColorBrush(color);
        }

        public static string CategoryIcon(string? category)
        {
            var c = category?.ToLowerInvariant() ?? string.Empty;
            if (c.Contains("ielts")) return "📝";
            if (c.Contains("toeic") || c.Contains("business")) return "💼";
            if (c.Contains("daily") || c.Contains("conversation")) return "💬";
            return "📚";
        }
    }
}
