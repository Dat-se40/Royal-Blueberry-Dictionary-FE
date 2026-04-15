using System.IO;
using System.Text.Json;
using Royal_Blueberry_Dictionary.Model;

namespace Royal_Blueberry_Dictionary.Service
{
    /// <summary>
    /// Cache JSON <see cref="PackageDetail"/> từ API backend để dùng khi không có mạng.
    /// </summary>
    public sealed class OfflinePackageCacheService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private readonly string _rootDir;

        public OfflinePackageCacheService()
        {
            _rootDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RoyalBlueberryDictionary",
                "offline_packages");
            Directory.CreateDirectory(_rootDir);
        }

        public string GetCachePath(string packageId) =>
            Path.Combine(_rootDir, $"{SanitizeFileName(packageId)}.json");

        private static string SanitizeFileName(string id)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                id = id.Replace(c, '_');
            return string.IsNullOrWhiteSpace(id) ? "package" : id;
        }

        public bool IsCached(string packageId) => File.Exists(GetCachePath(packageId));

        public DateTime? GetCachedWriteTimeUtc(string packageId)
        {
            var p = GetCachePath(packageId);
            return File.Exists(p) ? File.GetLastWriteTimeUtc(p) : null;
        }

        public long? GetCachedSizeBytes(string packageId)
        {
            var p = GetCachePath(packageId);
            return File.Exists(p) ? new FileInfo(p).Length : null;
        }

        public async Task SaveAsync(string packageId, PackageDetail detail, CancellationToken ct = default)
        {
            var path = GetCachePath(packageId);
            var json = JsonSerializer.Serialize(detail, JsonOptions);
            await File.WriteAllTextAsync(path, json, ct).ConfigureAwait(false);
        }

        public async Task<PackageDetail?> TryLoadAsync(string packageId, CancellationToken ct = default)
        {
            var path = GetCachePath(packageId);
            if (!File.Exists(path))
                return null;

            try
            {
                await using var fs = File.OpenRead(path);
                return await JsonSerializer.DeserializeAsync<PackageDetail>(fs, JsonOptions, ct)
                    .ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        public Task DeleteAsync(string packageId)
        {
            var path = GetCachePath(packageId);
            if (File.Exists(path))
                File.Delete(path);
            return Task.CompletedTask;
        }
    }
}
