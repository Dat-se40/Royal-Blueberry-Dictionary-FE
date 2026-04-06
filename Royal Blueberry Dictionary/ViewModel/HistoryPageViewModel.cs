using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.User_Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using INavigationAware = Royal_Blueberry_Dictionary.Service.INavigationAware;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class HistoryPageViewModel : ObservableObject, INavigationAware
    {
        SearchService _searchService;
        WordService _wordService;       
        [ObservableProperty] private ObservableCollection<WordEntry> _historyWords = new();
        public HistoryPageViewModel(SearchService searchService , WordService wordService)
        {
            this._searchService = searchService;
            this._wordService = wordService;
        }

        public void OnNavigatedFrom()
        {
            throw new NotImplementedException();
        }

        private async Task GetDataAsync()
        {
            var result = new List<WordEntry>();
            var cacheList = _searchService.getHistroyCacheToday().ToList();

            // Dùng foreach cơ bản để await tuần tự
            foreach (var dt in cacheList)
            {
                var res = await _wordService.GetWordEntryByDetail(dt, 0, 0);
                if (res != null) result.Add(res);
            }

            HistoryWords.Clear();
            foreach (var item in result)
            {
                HistoryWords.Add(item);
            }

            Console.WriteLine("[History page]:" + HistoryWords.Count);
        }
        public void OnNavigatedTo(object parameter)
        {
            _ = GetDataAsync(); 
        }
        [RelayCommand]
        public void RemoveFromHistory(WordEntry? wordEntry)
        {
            if (wordEntry == null) return;
            _searchService.RemoveWordInCache(wordEntry.Word);
            HistoryWords.Remove(wordEntry);

            Console.WriteLine($"[History]: Đã xóa từ {wordEntry.Word}");
        }
        [RelayCommand]
        public async Task FavoriteAsync(WordEntry? wordEntry)
        {
            if (wordEntry == null) return;
            await _wordService.FavoriteAsync(wordEntry);
        }
    }
}
