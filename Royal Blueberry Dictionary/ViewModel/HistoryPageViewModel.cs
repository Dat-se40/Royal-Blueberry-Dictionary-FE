using CommunityToolkit.Mvvm.ComponentModel;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
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

        [ObservableProperty] private ObservableCollection<WordEntry> _historyWords = new();
        public HistoryPageViewModel(SearchService searchService)
        {
            this._searchService = searchService;
        }

        public void OnNavigatedFrom()
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedTo(object parameter)
        {
            GetData();
        }
        private void GetData()
        {
            HistoryWords.Clear();
            var cache = _searchService.getHistroyCache(); // Giả sử trả về List<WordEntry>
            HistoryWords = new ObservableCollection<WordEntry>(cache);
            Console.WriteLine("[History page]:" + HistoryWords.Count);
        }
   
        private void UpdateUI() 
        {

        }
    }
}
