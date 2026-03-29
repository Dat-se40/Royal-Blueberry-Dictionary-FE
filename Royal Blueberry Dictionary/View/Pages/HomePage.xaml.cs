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

namespace Royal_Blueberry_Dictionary.View.Pages
{
    using System.Windows.Controls;

    // [NOTE] Các using sau đã bị tạm xóa vì liên quan đến Service/Data/Model:
    // - BlueBerryDictionary.Data (FileStorage)
    // - BlueBerryDictionary.Models (Quote, Word, ShortenWord)
    // - BlueBerryDictionary.Views.UserControls (WordItem)
    // - System.Diagnostics (Process)
    // - System.IO (Directory)
    // - System.Windows.Media.Imaging (BitmapImage)
    // TODO: Cập nhật namespace sang Royal_Blueberry_Dictionary tương ứng khi tích hợp

    public partial class HomePage : Page
    {
        // [NOTE] Đã xóa:
        // - Action<object, RoutedEventArgs> Navigate
        // - List<Quote> listQuotes
        // TODO: Khôi phục khi tích hợp navigate và quote data

        public HomePage()
        {
            InitializeComponent();

            // [NOTE] Đã xóa:
            // - Navigate += navigate
            // - listQuotes = new List<Quote>()
            // - this.Loaded += Home_Loaded1
            // - _ = InitializeAsync()
            // TODO: Khôi phục toàn bộ khi Service sẵn sàng
        }

        // [NOTE] Các method sau đã bị xóa hoàn toàn:
        // - ButtnNavigate_Click()          → TODO: Gắn lại Click trên toolbar buttons
        // - Hyperlink_RequestNavigate()    → TODO: Gắn lại RequestNavigate trên Hyperlink
        // - LoadData()                     → TODO: Gọi LoadRandomContent()
        // - Home_Loaded1()                 → TODO: Gắn lại Loaded event
        // - InitializeAsync()              → TODO: Gọi lại trong constructor
        // - AddWordItem(Word)              → TODO: Khôi phục khi có Word model
        // - AddWordItem(List<ShortenWord>) → TODO: Khôi phục + OnWordClick binding
        // - LoadAllQuotes()                → TODO: Khôi phục FileStorage.LoadQuoteAsync
        // - LoadRandomContent()            → TODO: Khôi phục Random quote logic
        // - LoadContent(int ID)            → TODO: Khôi phục QuoteText/QuoteAuthor/QuoteImage
    }
}