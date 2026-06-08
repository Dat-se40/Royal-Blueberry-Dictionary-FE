using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using INavigationAware = Royal_Blueberry_Dictionary.Service.INavigationAware;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class OfflinePackageRowViewModel : ObservableObject
    {
        private readonly OfflinePackagesPageViewModel _host;

        public Package Package { get; }

        [ObservableProperty] private bool _isDownloaded;

        public string PackageName => Package.name;
        public string Name => Package.name;
        public string Description => Package.description ?? string.Empty;
        public string Icon => PackageService.CategoryIcon(Package.category);
        public string TotalItems => Package.totalWords.ToString();
        public string SizeText { get; }
        public string Level => Package.level ?? "—";
        public string BadgeText => string.IsNullOrWhiteSpace(Package.category) ? "Package" : Package.category;
        public Brush BadgeColorBrush { get; }
        public string DownloadDate { get; }

        public OfflinePackageRowViewModel(
            OfflinePackagesPageViewModel host,
            Package package,
            bool isCached,
            long? cachedSizeBytes,
            DateTime? cachedUtc)
        {
            _host = host;
            Package = package;
            _isDownloaded = isCached;
            BadgeColorBrush = PackageService.CategoryBrush(Package.category);
            DownloadDate = cachedUtc.HasValue
                ? cachedUtc.Value.ToLocalTime().ToString("g")
                : string.Empty;

            if (cachedSizeBytes.HasValue)
            {
                var b = cachedSizeBytes.Value;
                SizeText = b >= 1_000_000
                    ? $"{b / 1_000_000.0:F1} MB"
                    : $"{b / 1_000.0:F0} KB";
            }
            else
            {
                SizeText = $"~{Math.Max(1, Package.totalWords) * 200 / 1024} KB";
            }
        }

        [RelayCommand]
        private Task DownloadAsync() => _host.DownloadPackageAsync(Package);

        [RelayCommand]
        private Task PreviewAsync() => _host.PreviewPackageAsync(Package);

        [RelayCommand]
        private Task DeleteOfflineAsync() => _host.DeleteOfflineAsync(Package);

        [RelayCommand]
        private Task ImportAsync() => _host.ImportPackageAsync(Package);
    }

    public partial class OfflinePackagesPageViewModel : ObservableObject, INavigationAware
    {
        private readonly PackageService _packageService;

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private ObservableCollection<OfflinePackageRowViewModel> _downloadedPackages = new();
        [ObservableProperty] private ObservableCollection<OfflinePackageRowViewModel> _availablePackages = new();
        [ObservableProperty] private int _downloadedCount;
        [ObservableProperty] private int _availableCount;
        [ObservableProperty] private string _totalWordsDisplay = "0";
        [ObservableProperty] private string _totalSizeDisplay = "0 KB";
        [ObservableProperty] private bool _showDownloadedEmpty = true;

        public OfflinePackagesPageViewModel(PackageService packageService)
        {
            _packageService = packageService;
        }

        public void OnNavigatedFrom() { }

        public void OnNavigatedTo(object parameter) => _ = LoadAsync();

        [RelayCommand]
        private Task RefreshAsync() => LoadAsync();

        public async Task LoadAsync()
        {
            IsLoading = true;
            StatusMessage = string.Empty;
            try
            {
                var list = await _packageService.getAllPackages();
                DownloadedPackages.Clear();
                AvailablePackages.Clear();

                foreach (var p in list.OrderBy(x => x.category).ThenBy(x => x.name))
                {
                    var cached = _packageService.IsOfflineCached(p.Id);
                    var utc = _packageService.GetOfflineCachedAtUtc(p.Id);
                    var size = _packageService.GetOfflineCachedSizeBytes(p.Id);
                    var row = new OfflinePackageRowViewModel(this, p, cached, size, utc);
                    if (cached)
                        DownloadedPackages.Add(row);
                    else
                        AvailablePackages.Add(row);
                }

                AvailableCount = list.Count;
                DownloadedCount = DownloadedPackages.Count;
                ShowDownloadedEmpty = DownloadedCount == 0;

                var totalWords = DownloadedPackages.Sum(r => r.Package.totalWords);
                TotalWordsDisplay = totalWords >= 1000
                    ? $"{totalWords / 1000.0:F1}K"
                    : totalWords.ToString();

                long totalBytes = 0;
                foreach (var r in DownloadedPackages)
                {
                    var b = _packageService.GetOfflineCachedSizeBytes(r.Package.Id);
                    if (b.HasValue) totalBytes += b.Value;
                }

                TotalSizeDisplay = totalBytes >= 1_000_000
                    ? $"{totalBytes / 1_000_000.0:F1} MB"
                    : totalBytes >= 1_000
                        ? $"{totalBytes / 1_000.0:F0} KB"
                        : totalBytes > 0 ? $"{totalBytes} B" : "0 KB";

                if (list.Count == 0)
                    StatusMessage = "No packages found or unable to connect to the API.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        internal async Task DownloadPackageAsync(Package package)
        {
            try
            {
                var ok = await _packageService.DownloadForOfflineAsync(package.Id);
                if (!ok)
                {
                    MessageBox.Show(
                        "Could not download package details. Ensure the backend is running and GET /api/packages/details/{id} returns data.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show($"Saved \"{package.name}\" for offline use.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal async Task PreviewPackageAsync(Package package)
        {
            try
            {
                var detail = await _packageService.GetPackageDetailPreferNetworkAsync(package.Id);
                if (detail == null)
                {
                    MessageBox.Show(
                        "No data available. Download the package first or connect to the API.",
                        "Notice",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                var dlg = new PackageDetailsDialog(package, detail)
                {
                    Owner = Application.Current.MainWindow
                };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal async Task DeleteOfflineAsync(Package package)
        {
            if (MessageBox.Show(
                    $"Remove offline cache for \"{package.name}\"?\n(Words already imported into My Words will not be deleted.)",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            await _packageService.RemoveOfflineCacheAsync(package.Id);
            await LoadAsync();
        }

        internal async Task ImportPackageAsync(Package package)
        {
            if (!_packageService.IsOfflineCached(package.Id))
            {
                MessageBox.Show("Please download the package to your device before importing.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var detail = await _packageService.GetCachedDetailAsync(package.Id);
            if (detail?.Words == null || detail.Words.Count == 0)
            {
                MessageBox.Show("Cache file is invalid or empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var n = await _packageService.ImportPackageToMyWordsAsync(package, detail, App.UserId);
                MessageBox.Show(
                    $"Added {n} new word(s); existing words only receive the tag if they do not have it yet.",
                    "Done",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
