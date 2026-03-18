using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using System.Collections.ObjectModel;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class SearchViewModel : ObservableObject
    {
        private readonly SearchService _searchService;

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

        public SearchViewModel(SearchService searchService)
        {
            _searchService = searchService;
        }

        #region Logic Hooks

        // Hook tự động của CommunityToolkit khi SearchText thay đổi
        partial void OnSearchTextChanged(string value)
        {
            // Gọi hàm xử lý Async mà không dùng async void trực tiếp ở hook
            _ = UpdateSuggestionsAsync(value);
        }

        private async Task UpdateSuggestionsAsync(string value)
        {
            //if (string.IsNullOrWhiteSpace(value))
            //{
            //    Suggestions.Clear();
            //    return;
            //}

            //// TODO: Thêm Task.Delay(300) ở đây để làm Debounce nếu gọi API
            //var results = await _searchService.GetSuggestionsAsync(value);

            //Suggestions.Clear();
            //foreach (var item in results) Suggestions.Add(item);
        }

        // Khi SearchResult có dữ liệu, tự động điều hướng
        partial void OnSearchResultChanged(WordDetail? value)
        {
            if (value != null)
            {
                ExecuteShowDetailPage();
            }
        }
        #endregion

        #region Commands

        [RelayCommand]
        public async Task ExecuteSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            IsSearching = true;
            BtnSearchText = "Searching...";

            try
            {
                // Chỉ cần cập nhật SearchResult, OnSearchResultChanged sẽ lo phần điều hướng
                SearchResult = await _searchService.searchAWord(SearchText);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SearchViewModel] Error: {e.Message}");
                // TODO: Thông báo lỗi cho người dùng qua UI
            }
            finally
            {
                IsSearching = false;
                BtnSearchText = "Search";
            }
        }

        private void ExecuteShowDetailPage()
        {
            // navigationService.NavigateTo("DetailPage", SearchResult);
            Console.WriteLine("Navigating to detail page...");
        }
        #endregion
    }
}