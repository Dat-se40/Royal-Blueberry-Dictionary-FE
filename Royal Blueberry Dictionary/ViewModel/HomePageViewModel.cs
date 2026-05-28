using BlueBerryDictionary.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using System.Collections.ObjectModel;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public enum VocabSortMode { Recent, AtoZ, ZtoA }

    public partial class HomePageViewModel : ObservableObject, Service.INavigationAware
    {
        private readonly IWordEntryRepository _wordRepo;
        private readonly ITagRepository _tagRepo;
        private readonly SearchService _searchService;
        private readonly NavigationService _navigationService;
        private readonly AuthService _authService;

        private static readonly List<(string Text, string Author)> Quotes =
        [
            ("The limits of my language mean the limits of my world.", "Ludwig Wittgenstein"),
            ("One language sets you in a corridor for life. Two languages open every door along the way.", "Frank Smith"),
            ("To have another language is to possess a second soul.", "Charlemagne"),
            ("A different language is a different vision of life.", "Federico Fellini"),
            ("The more that you read, the more things you will know.", "Dr. Seuss"),
            ("Language is the road map of a culture.", "Rita Mae Brown"),
            ("Words are the most powerful drug used by mankind.", "Rudyard Kipling"),
            ("You can never understand one language until you understand at least two.", "Geoffrey Willans"),
        ];

        // Stats
        [ObservableProperty] private int _totalWords;
        [ObservableProperty] private int _favoriteCount;
        [ObservableProperty] private int _tagCount;

        // Quote
        [ObservableProperty] private string _quoteText = $"\"{Quotes[0].Text}\"";
        [ObservableProperty] private string _quoteAuthor = $"— {Quotes[0].Author}";

        // Greeting
        [ObservableProperty] private string _userGreeting = "Welcome back!";
        [ObservableProperty] private string _userDisplayName = string.Empty;
        [ObservableProperty] private string _userInitials = "?";
        [ObservableProperty] private string _avatarUrl = string.Empty;
        [ObservableProperty] private bool _hasAvatar;

        // Recent searches
        [ObservableProperty] private ObservableCollection<string> _recentSearchWords = [];
        [ObservableProperty] private bool _hasRecentSearches;

        // Vocabulary
        [ObservableProperty] private ObservableCollection<WordEntry> _recentWords = [];
        [ObservableProperty] private WordEntry? _wordOfTheDay;
        [ObservableProperty] private bool _hasWords;
        [ObservableProperty] private VocabSortMode _sortMode = VocabSortMode.Recent;
        private List<WordEntry> _allUniqueWords = [];

        // Weekly chart
        [ObservableProperty] private ObservableCollection<DayBarItem> _weeklyChart = [];
        [ObservableProperty] private bool _hasChartData;

        // Loading
        [ObservableProperty] private bool _isLoading;

        public HomePageViewModel(
            IWordEntryRepository wordRepo,
            ITagRepository tagRepo,
            SearchService searchService,
            NavigationService navigationService,
            AuthService authService)
        {
            _wordRepo = wordRepo;
            _tagRepo = tagRepo;
            _searchService = searchService;
            _navigationService = navigationService;
            _authService = authService;
        }

        public async void OnNavigatedTo(object parameter) => await LoadAsync();
        public void OnNavigatedFrom() { }

        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var userId = App.UserId;
                var allWords = await _wordRepo.GetAllAsync(userId);
                var favorites = await _wordRepo.GetFavoritedAsync(userId);
                var tags = await _tagRepo.GetAllTagsAsync();

                // Stats
                TotalWords = allWords.Count;
                FavoriteCount = favorites.Count;
                TagCount = tags.Count;
                HasWords = allWords.Count > 0;

                // Quote (random)
                var rng = new Random();
                var q = Quotes[rng.Next(Quotes.Count)];
                QuoteText = $"\"{q.Text}\"";
                QuoteAuthor = $"— {q.Author}";

                // Greeting
                var user = _authService.CurrentUser;
                if (user != null)
                {
                    var firstName = user.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? user.Name;
                    UserDisplayName = firstName;
                    var initParts = user.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2);
                    UserInitials = string.Concat(initParts.Select(p => p[0])).ToUpper();
                    AvatarUrl = user.AvatarUrl ?? string.Empty;
                    HasAvatar = !string.IsNullOrWhiteSpace(AvatarUrl);
                }
                else
                {
                    UserDisplayName = "Guest";
                    UserInitials = "G";
                    HasAvatar = false;
                }
                var hour = DateTime.Now.Hour;
                var greeting = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
                UserGreeting = $"{greeting},";

                // Recent searches
                var searches = _searchService.getHistroyCacheToday()
                    .Select(d => d.Word)
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(8)
                    .ToList();
                RecentSearchWords.Clear();
                foreach (var w in searches) RecentSearchWords.Add(w);
                HasRecentSearches = RecentSearchWords.Count > 0;

                // Deduplicate by word (case-insensitive), keep most-recently-modified
                _allUniqueWords = allWords
                    .GroupBy(w => w.Word.Trim().ToLowerInvariant())
                    .Select(g => g.OrderByDescending(w => w.LastModifiedAt).First())
                    .ToList();

                // Word of the Day (from deduped list)
                if (_allUniqueWords.Count > 0)
                    WordOfTheDay = _allUniqueWords[rng.Next(_allUniqueWords.Count)];

                // Apply initial sort
                ApplySort();

                // Weekly chart (last 7 days, using full list for counts)
                var today = DateTime.Today;
                var days = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
                var grouped = allWords
                    .GroupBy(w => w.LastModifiedAt.ToLocalTime().Date)
                    .ToDictionary(g => g.Key, g => g.Count());
                var counts = days.Select(d => grouped.TryGetValue(d, out var c) ? c : 0).ToList();
                var maxCount = counts.Max() > 0 ? counts.Max() : 1;

                WeeklyChart.Clear();
                for (int i = 0; i < 7; i++)
                {
                    WeeklyChart.Add(new DayBarItem
                    {
                        Day = days[i].ToString("ddd"),
                        Count = counts[i],
                        BarHeight = Math.Max(4, counts[i] * 80.0 / maxCount),
                        IsToday = days[i] == today,
                    });
                }
                HasChartData = counts.Sum() > 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplySort()
        {
            IEnumerable<WordEntry> sorted = SortMode switch
            {
                VocabSortMode.AtoZ  => _allUniqueWords.OrderBy(w => w.Word, StringComparer.OrdinalIgnoreCase),
                VocabSortMode.ZtoA  => _allUniqueWords.OrderByDescending(w => w.Word, StringComparer.OrdinalIgnoreCase),
                _                   => _allUniqueWords.OrderByDescending(w => w.LastModifiedAt),
            };

            RecentWords.Clear();
            foreach (var w in sorted.Take(20))
                RecentWords.Add(w);

            HasWords = RecentWords.Count > 0;
        }

        [RelayCommand]
        private void SetSort(string? mode)
        {
            SortMode = mode switch
            {
                "az"   => VocabSortMode.AtoZ,
                "za"   => VocabSortMode.ZtoA,
                _      => VocabSortMode.Recent,
            };
            ApplySort();
        }

        [RelayCommand]
        private async Task OpenWordAsync(WordEntry? entry)
        {
            if (entry == null) return;
            var result = await _searchService.searchAWord(entry.Word);
            if (result != null && _searchService.IsValidWordDetail(result))
                _navigationService.NavigateTo<DetailsPage, DetailsPageViewModel>(result);
        }

        [RelayCommand]
        private async Task OpenWordOfTheDayAsync()
        {
            if (WordOfTheDay == null) return;
            await OpenWordAsync(WordOfTheDay);
        }

        [RelayCommand]
        private async Task OpenSearchWordAsync(string? word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;
            var result = await _searchService.searchAWord(word);
            if (result != null && _searchService.IsValidWordDetail(result))
                _navigationService.NavigateTo<DetailsPage, DetailsPageViewModel>(result);
        }
    }

    public class DayBarItem
    {
        public string Day { get; set; } = string.Empty;
        public int Count { get; set; }
        public double BarHeight { get; set; }
        public bool IsToday { get; set; }
    }

}
