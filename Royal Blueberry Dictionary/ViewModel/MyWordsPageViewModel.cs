using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Repository.Interface;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using INavigationAware = Royal_Blueberry_Dictionary.Service.INavigationAware;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public sealed class TagRowViewModel
    {
        public Tag Tag { get; }
        public int WordCount { get; }

        public TagRowViewModel(Tag tag, int wordCount)
        {
            Tag = tag;
            WordCount = wordCount;
        }
    }

    public partial class AlphabetItem : ObservableObject
    {
        [ObservableProperty] private string _letter = string.Empty;
        [ObservableProperty] private int _wordCount;
        [ObservableProperty] private bool _isActive;
        [ObservableProperty] private bool _isAvailable;

        public string DisplayText => WordCount > 0 ? $"{Letter} ({WordCount})" : Letter;
    }

    public partial class MyWordsPageViewModel : ObservableObject, INavigationAware
    {
        private string UserId => App.UserId; 

        private readonly TagService _tagService;
        private readonly WordService _wordService;
        private List<WordEntry> _allEntries = new();

        [ObservableProperty] private string _currFilter = "All";
        [ObservableProperty] private int _wordsCount;
        [ObservableProperty] private ObservableCollection<TagRowViewModel> _tagRows = new();
        [ObservableProperty] private ObservableCollection<WordEntry> _filteredWords = new();
        [ObservableProperty] private Tag? _selectedTag;
        [ObservableProperty] private string _selectedLetter = "ALL";
        [ObservableProperty] private string? _selectedPartOfSpeech;
        [ObservableProperty] private int _totalWords;
        [ObservableProperty] private int _totalTags;
        [ObservableProperty] private int _wordsThisWeek;
        [ObservableProperty] private int _wordsThisMonth;
        [ObservableProperty] private ObservableCollection<AlphabetItem> _alphabetItems = new();

        public MyWordsPageViewModel(TagService tagService, WordService wordService)
        {
            _tagService = tagService;
            _wordService = wordService;
        }

        public void OnNavigatedFrom() { }

        public void OnNavigatedTo(object parameter)
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataCoreAsync()
        {
            _allEntries = await _tagService.GetAllWordEntriesAsync(UserId);

            TagRows.Clear();
            var tagPairs = await _tagService.GetTagsWithRelationCountsAsync(UserId);
            foreach (var (tag, count) in tagPairs)
                TagRows.Add(new TagRowViewModel(tag, count));

            UpdateStatistics();
            LoadAlphabetDistribution();
        }

        private async Task LoadDataAsync()
        {
            await LoadDataCoreAsync();
            await ApplyFiltersAsync();
        }

        private void LoadAlphabetDistribution()
        {
            var distribution = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in _allEntries)
            {
                var w = e.Word?.Trim();
                if (string.IsNullOrEmpty(w)) continue;
                var first = char.ToUpperInvariant(w[0]);
                if (first < 'A' || first > 'Z') continue;
                var key = first.ToString();
                distribution[key] = distribution.GetValueOrDefault(key, 0) + 1;
            }

            AlphabetItems.Clear();
            AlphabetItems.Add(new AlphabetItem
            {
                Letter = "ALL",
                WordCount = _allEntries.Count,
                IsActive = SelectedLetter == "ALL",
                IsAvailable = true
            });

            for (var c = 'A'; c <= 'Z'; c++)
            {
                var letter = c.ToString();
                var count = distribution.GetValueOrDefault(letter, 0);
                AlphabetItems.Add(new AlphabetItem
                {
                    Letter = letter,
                    WordCount = count,
                    IsActive = SelectedLetter == letter,
                    IsAvailable = count > 0
                });
            }
        }

        private void UpdateStatistics()
        {
            TotalWords = _allEntries.Count;
            TotalTags = TagRows.Count;
            var now = DateTime.UtcNow;
            WordsThisWeek = _allEntries.Count(e => e.LastModifiedAt >= now.AddDays(-7));
            WordsThisMonth = _allEntries.Count(e => e.LastModifiedAt >= new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        private async Task ApplyFiltersAsync()
        {
            List<WordEntry> words;
            if (SelectedTag != null)
                words = await _tagService.GetWordsInTagAsync(UserId, SelectedTag.Id);
            else
                words = _allEntries.ToList();

            if (!string.IsNullOrEmpty(SelectedLetter) && !string.Equals(SelectedLetter, "ALL", StringComparison.OrdinalIgnoreCase))
                words = words.Where(w =>
                        w.Word.StartsWith(SelectedLetter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(SelectedPartOfSpeech))
                words = words.Where(w =>
                        string.Equals(w.PartOfSpeech, SelectedPartOfSpeech, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var filterParts = new List<string>();
            if (SelectedTag != null) filterParts.Add(SelectedTag.Name);
            if (!string.IsNullOrEmpty(SelectedLetter) && !string.Equals(SelectedLetter, "ALL", StringComparison.OrdinalIgnoreCase))
                filterParts.Add(SelectedLetter.ToUpperInvariant());
            if (!string.IsNullOrEmpty(SelectedPartOfSpeech))
                filterParts.Add(SelectedPartOfSpeech);

            CurrFilter = filterParts.Count > 0 ? string.Join(" + ", filterParts) : "All";

            FilteredWords.Clear();
            foreach (var w in words.OrderBy(w => w.Word, StringComparer.OrdinalIgnoreCase))
                FilteredWords.Add(w);

            WordsCount = FilteredWords.Count;
        }

        [RelayCommand]
        private async Task OpenRemoveTagDialogAsync()
        {
            var dialog = new RemoveTagDialog(UserId);
            if (dialog.ShowDialog() == true)
            {
                SelectedTag = null;
                await LoadDataAsync();
            }
        }

        [RelayCommand]
        private async Task FilterByTagAsync(Tag? tag)
        {
            SelectedTag = tag;
            await ApplyFiltersAsync();
        }

        [RelayCommand]
        private async Task FilterByLetterAsync(string? letter)
        {
            SelectedLetter = string.IsNullOrEmpty(letter) ? "ALL" : letter;
            foreach (var item in AlphabetItems)
                item.IsActive = string.Equals(item.Letter, SelectedLetter, StringComparison.OrdinalIgnoreCase);
            await ApplyFiltersAsync();
        }

        /// <summary>Chữ cái A–Z hoặc ALL (popup — ALL reset toàn bộ filter như repo cũ).</summary>
        [RelayCommand]
        private async Task PickAlphabetAsync(string? letter)
        {
            if (string.IsNullOrEmpty(letter) || string.Equals(letter, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                await ClearFiltersAsync();
                return;
            }
            await FilterByLetterAsync(letter);
        }

        [RelayCommand]
        private async Task FilterByPartOfSpeechAsync(string? pos)
        {
            SelectedPartOfSpeech = pos;
            await ApplyFiltersAsync();
        }

        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            SelectedTag = null;
            SelectedLetter = "ALL";
            SelectedPartOfSpeech = null;
            foreach (var item in AlphabetItems)
                item.IsActive = string.Equals(item.Letter, "ALL", StringComparison.OrdinalIgnoreCase);
            CurrFilter = "All";
            await ApplyFiltersAsync();
        }

        [RelayCommand]
        private async Task CreateTagAsync()
        {
            var dialog = new TagPickerDialog(UserId);
            if (dialog.ShowDialog() == true)
                await LoadDataAsync();
        }

        [RelayCommand]
        private void AddWord()
        {
            MessageBox.Show("The add new word feature is under development.", "My Words",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task DeleteWordEntryAsync(WordEntry? wordEntry)
        {
            if (wordEntry == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{wordEntry.Word}'?\n\nThis will also remove it from all tags.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var tagRepo = App.serviceProvider
                    .GetRequiredService<ITagRepository>();

                await tagRepo.DeleteRelationsByWordAsync(UserId, wordEntry.Word, wordEntry.MeaningIndex);
                await tagRepo.SaveChangesAsync();

                await _wordService.DeleteWordEntryAsync(wordEntry.Id);

                var savedTag = SelectedTag;
                var savedLetter = SelectedLetter;
                var savedPos = SelectedPartOfSpeech;

                await LoadDataCoreAsync();

                SelectedTag = savedTag != null && TagRows.Any(r => r.Tag.Id == savedTag.Id) ? savedTag : null;
                SelectedLetter = savedLetter;
                SelectedPartOfSpeech = savedPos;

                await ApplyFiltersAsync();

                MessageBox.Show($"✅ Deleted '{wordEntry.Word}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error deleting entry: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task FavoriteAsync(WordEntry? wordEntry)
        {
            if (wordEntry == null) return;
            await _wordService.FavoriteAsync(wordEntry);
        }
    }
}
