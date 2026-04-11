using BlueBerryDictionary.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using NavigationService = Royal_Blueberry_Dictionary.Service.NavigationService;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class SearchViewModel : ObservableObject, Service.INavigationAware
    {
        private readonly SearchService _searchService;
        private readonly NavigationService _navigationService;
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
            var results = await _searchService.GetSuggestionsAsync(value);

            Suggestions.Clear();
            foreach (var item in results) Suggestions.Add(item);
            IsSuggestionsOpen = Suggestions.Count > 0;
        }

        // Khi SearchResult có dữ liệu, tự động điều hướng
        #endregion

        #region Commands

        [RelayCommand]
        public async Task ExecuteSearchAsync(string? targetWord)
        {
            string wordToSearch = targetWord ?? SearchText;
            StatusText = "Searching"; 
            if (string.IsNullOrWhiteSpace(wordToSearch)) return;
            try
            {
                Console.WriteLine($"Searching : {wordToSearch}");
                IsSearching = true;
                var result = await _searchService.searchAWord(wordToSearch);
                if (result != null && _searchService.IsValidWordDetail(result))
                {
                    SearchResult = result;
                    NavigateToDetailsPage(result);
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
        public void NavigateToDetailsPage(WordDetail wordDetail)
        {
            if (wordDetail == null) 
            {
                Console.WriteLine("have no data to load");
            }
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