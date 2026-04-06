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
    public partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            InitializeComponent();

        }
    }
}