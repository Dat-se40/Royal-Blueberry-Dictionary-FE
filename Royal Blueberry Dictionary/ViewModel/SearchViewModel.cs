using BlueBerryDictionary.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using NavigationService = Royal_Blueberry_Dictionary.Service.NavigationService;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class SearchViewModel : ObservableObject, Service.INavigationAware
    {
        private readonly SearchService _searchService;
        private readonly NavigationService _navigationService;
        private CancellationTokenSource? _suggestionCts;
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isSearching = false;

        [ObservableProperty]
        private string _btnSearchText = "Search";

        [ObservableProperty]
        private ObservableCollection<string> _suggestions = new ObservableCollection<string>();

        [ObservableProperty]
        private WordDetail? _searchResult;
        [ObservableProperty]
        private bool _isSuggestionsOpen = false;

        [ObservableProperty]
        private int _selectedSuggestionIndex = -1;

        [ObservableProperty]
        private string _statusText ="Search";  
        public SearchViewModel(SearchService searchService, Service.NavigationService navigationService )
        {
            _searchService = searchService;
            _navigationService = navigationService;
        }

        #region Logic Hooks

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Suggestions.Clear();
                IsSuggestionsOpen = false;
                return;
            }
            _ = UpdateSuggestionsAsync(value);
        }
        // partial void On<Properties Name>Change
        private async Task UpdateSuggestionsAsync(string value)
        {
            _suggestionCts?.Cancel();
            _suggestionCts?.Dispose();
            _suggestionCts = new CancellationTokenSource();
            var token = _suggestionCts.Token;

            try
            {
                await Task.Delay(200, token);
                var results = await _searchService.GetSuggestionsAsync(value);
                if (token.IsCancellationRequested) return;

                Suggestions.Clear();
                foreach (var item in results) Suggestions.Add(item);
                IsSuggestionsOpen = Suggestions.Count > 0;
                SelectedSuggestionIndex = Suggestions.Count > 0 ? 0 : -1;
            }
            catch (TaskCanceledException)
            {
            }
        }

        partial void OnIsSuggestionsOpenChanged(bool value)
        {
            if (!value)
            {
                SelectedSuggestionIndex = -1;
            }
        }

        // Khi SearchResult có dữ liệu, tự động điều hướng
        #endregion

        #region Commands

        [RelayCommand]
        public async Task ExecuteSearchAsync(string? targetWord)
        {
            if (IsSuggestionsOpen &&
                SelectedSuggestionIndex >= 0 &&
                SelectedSuggestionIndex < Suggestions.Count &&
                string.IsNullOrEmpty(targetWord))
            {
                await SelectSuggestionAsync(Suggestions[SelectedSuggestionIndex]);
                return;
            }

            string wordToSearch = targetWord ?? SearchText;
            StatusText = "Searching"; 
            if (string.IsNullOrWhiteSpace(wordToSearch)) return;
            try
            {
                IsSearching = true;
                IsSuggestionsOpen = false;
                var result = await _searchService.searchAWord(wordToSearch);
                if (result != null && _searchService.IsValidWordDetail(result))
                {
                    SearchResult = result;
                    NavigateToDetailsPage(result);
                }
                else
                {
                    MessageBox.Show(
                        $"No definition found for \"{wordToSearch}\".",
                        "Search",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            finally
            {
                IsSearching = false;
            }
            StatusText = "Search";
        }
        // Command khi người dùng Click vào một dòng trong ListBox gợi ý
        [RelayCommand]
        public async Task SelectSuggestionAsync(string selectedWord)
        {
            if (string.IsNullOrEmpty(selectedWord)) return;

            SearchText = selectedWord; // Cập nhật text lên ô search
            IsSuggestionsOpen = false;  // Đóng popup

            await ExecuteSearchAsync(selectedWord); // Tiến hành search luôn
        }

        [RelayCommand]
        private void MoveSuggestionDown()
        {
            if (!IsSuggestionsOpen || Suggestions.Count == 0)
            {
                return;
            }

            SelectedSuggestionIndex = SelectedSuggestionIndex < 0
                ? 0
                : Math.Min(SelectedSuggestionIndex + 1, Suggestions.Count - 1);
        }

        [RelayCommand]
        private void MoveSuggestionUp()
        {
            if (!IsSuggestionsOpen || Suggestions.Count == 0)
            {
                return;
            }

            SelectedSuggestionIndex = SelectedSuggestionIndex < 0
                ? Suggestions.Count - 1
                : Math.Max(SelectedSuggestionIndex - 1, 0);
        }

        public void NavigateToDetailsPage(WordDetail? wordDetail)
        {
            if (wordDetail == null) return;
            _navigationService.NavigateTo<DetailsPage, DetailsPageViewModel>(wordDetail);
        }

        public void OnNavigatedTo(object parameter)
        {
            Console.WriteLine("Ddang qua ben detail");
        }

        public void OnNavigatedFrom()
        {
         
        }
        #endregion
    }
}