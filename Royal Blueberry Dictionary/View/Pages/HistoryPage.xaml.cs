using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

// [NOTE] Các using sau đã bị tạm xóa vì liên quan đến Service/Model:
// - Models (CacheEntry)
// - Services (WordCacheManager, TagService)
// - System.Collections.ObjectModel (ObservableCollection)
// TODO: Khôi phục khi tích hợp lại ViewModel + Service

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class HistoryPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // [NOTE] Đã xóa:
        // - WordCacheManager _wordCacheManager
        // - ObservableCollection<CacheEntry> HistoryItems
        // TODO: Khôi phục cùng với WordCacheManager.Instance khi Service sẵn sàng

        public HistoryPage()
        {
            InitializeComponent();

            // [NOTE] Đã xóa: _wordCacheManager = WordCacheManager.Instance;
            // TODO: Gọi LoadData() sau khi Service sẵn sàng
        }

        public void LoadData()
        {
            // [NOTE] Đã xóa:
            // var caches = _wordCacheManager.GetAllCacheEntries();
            // HistoryItems = new ObservableCollection<CacheEntry>(caches);
            // TODO: Khôi phục GetAllCacheEntries() → HistoryItems → LoadDefCards()

            LoadDefCards();
        }

        private void LoadDefCards()
        {
            mainContent.Children.Clear();

            // [NOTE] Đã xóa vòng lặp foreach (var item in HistoryItems)
            // TODO: Khôi phục khi HistoryItems có dữ liệu:
            // - newCard.TimeStamp = item._lastAccessed.ToShortTimeString()
            // - newCard.MouseDown → HandleWordClick(word)
            // - TagService.Instance.FindWordInsensitive (đã comment sẵn trong bản gốc)
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}