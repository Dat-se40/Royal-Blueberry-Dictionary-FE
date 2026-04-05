using Royal_Blueberry_Dictionary.Model;
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

namespace Royal_Blueberry_Dictionary.View.User_Control
{
    public partial class WordDefinitionCard : UserControl
    {
        // =========================
        // Dependency Properties
        // =========================

        public static readonly DependencyProperty WordProperty =
            DependencyProperty.Register(nameof(Word), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PronunciationProperty =
            DependencyProperty.Register(nameof(Pronunciation), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PartOfSpeechProperty =
            DependencyProperty.Register(nameof(PartOfSpeech), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefinitionProperty =
            DependencyProperty.Register(nameof(Definition), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TimeStampProperty =
            DependencyProperty.Register(nameof(TimeStamp), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ViewCountProperty =
            DependencyProperty.Register(nameof(ViewCount), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RegionProperty =
            DependencyProperty.Register(nameof(Region), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Example1Property =
            DependencyProperty.Register(nameof(Example1), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Example2Property =
            DependencyProperty.Register(nameof(Example2), typeof(string), typeof(WordDefinitionCard), new PropertyMetadata(string.Empty));

        // =========================
        // CLR Wrappers
        // =========================

        public string Word
        {
            get => (string)GetValue(WordProperty);
            set => SetValue(WordProperty, value);
        }

        public string Pronunciation
        {
            get => (string)GetValue(PronunciationProperty);
            set => SetValue(PronunciationProperty, value);
        }

        public string PartOfSpeech
        {
            get => (string)GetValue(PartOfSpeechProperty);
            set => SetValue(PartOfSpeechProperty, value);
        }

        public string Definition
        {
            get => (string)GetValue(DefinitionProperty);
            set => SetValue(DefinitionProperty, value);
        }

        public string TimeStamp
        {
            get => (string)GetValue(TimeStampProperty);
            set => SetValue(TimeStampProperty, value);
        }

        public string ViewCount
        {
            get => (string)GetValue(ViewCountProperty);
            set => SetValue(ViewCountProperty, value);
        }

        public string Region
        {
            get => (string)GetValue(RegionProperty);
            set => SetValue(RegionProperty, value);
        }

        public string Example1
        {
            get => (string)GetValue(Example1Property);
            set => SetValue(Example1Property, value);
        }

        public string Example2
        {
            get => (string)GetValue(Example2Property);
            set => SetValue(Example2Property, value);
        }

        // =========================
        // Constructor
        // =========================
        private WordEntry _wordEntry; 
        public WordDefinitionCard()
        {
            InitializeComponent();
            _wordEntry = new WordEntry();
        }
        public void LoadData(WordEntry wordEntry) 
        {
            if (wordEntry == null) return;
            this.Word  = wordEntry.Word ?? "NULL";
            this.Pronunciation = wordEntry.Phonetic ?? "NULL";  
            this.PartOfSpeech = wordEntry.PartOfSpeech ?? "NULL";   
            this.Definition = wordEntry.Definition ?? "NULL";   
            this.Region = "US";
            this.Example1 = wordEntry.Example ?? string.Empty;    
            this.Example2 = wordEntry.Note ?? string.Empty;
        }
        // =========================
        // UI Helper Methods
        // =========================

        public void UpdateExampleSection(
            string example1Text,
            string example2Text = "",
            string example1LabelText = "Example:",
            string example2LabelText = "Note:")
        {
            Example1 = example1Text ?? string.Empty;
            Example2 = example2Text ?? string.Empty;

            if (Example1Label != null)
                Example1Label.Text = example1LabelText;

            if (Example2Label != null)
                Example2Label.Text = example2LabelText;

            if (Example2Container != null)
            {
                Example2Container.Visibility =
                    string.IsNullOrWhiteSpace(Example2)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public void HideExample2()
        {
            if (Example2Container != null)
                Example2Container.Visibility = Visibility.Collapsed;
        }

        public void ShowExample2()
        {
            if (Example2Container != null)
                Example2Container.Visibility = Visibility.Visible;
        }

        public void SetExample1Label(string text)
        {
            if (Example1Label != null)
                Example1Label.Text = text;
        }

        public void SetExample2Label(string text)
        {
            if (Example2Label != null)
                Example2Label.Text = text;
        }

        // =========================
        // Commented Functional/Event Logic
        // =========================

        /*
        // Chức năng cũ:
        // Dùng để xử lý click favorite.
        // Liên quan tới state dữ liệu, service lưu trữ, và event nghiệp vụ.
        // Tạm bỏ vì bản này chỉ giữ FE/UI.
        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
        }
        */

        /*
        // Chức năng cũ:
        // Dùng để xử lý click save/download.
        // Liên quan tới file, service, MessageBox.
        // Tạm bỏ vì bản này chỉ giữ FE/UI.
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
        }
        */

        /*
        // Chức năng cũ:
        // Dùng để xử lý click delete.
        // Liên quan tới xóa dữ liệu, xóa file, gọi service, phát event.
        // Tạm bỏ vì bản này chỉ giữ FE/UI.
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
        }
        */

        /*
        // Chức năng cũ:
        // Dùng để cập nhật trạng thái yêu thích và đổi style nút favorite.
        // Nếu sau này muốn làm favorite UI-only thì có thể viết lại đơn giản hơn.
        private void OnIsFavoritedChanged()
        {
        }
        */

        /*
        // Chức năng cũ:
        // Dùng để xử lý dữ liệu Example/Note từ model cũ.
        // Có phụ thuộc dữ liệu business/model.
        // Bản FE hiện tại thay bằng UpdateExampleSection(...) để set UI trực tiếp.
        private void HandleExampleAndNote()
        {
        }
        */

        /*
        // Chức năng cũ:
        // Dùng để tăng view count khi click card.
        // Liên quan tới dữ liệu và nghiệp vụ thống kê.
        // Tạm bỏ vì bản này chỉ giữ FE/UI.
        private void UpdateViewCount()
        {
        }
        */

        /*
        // Event cũ:
        // public event EventHandler FavoriteClicked;
        // public event EventHandler DeleteClicked;
        // public event EventHandler CardClicked;
        //
        // Tạm bỏ toàn bộ vì đây là event nghiệp vụ / hành vi chức năng.
        // Nếu cần thì sau này chuyển sang Command hoặc event ở layer ngoài.
        */

        /*
        // Service / ViewModel / Model cũ:
        // - TagService
        // - FileStorage
        // - Word
        // - WordShortened
        // - các constructor nhận model
        //
        // Tạm bỏ qua trong bản này theo yêu cầu.
        // Nếu cần tái kết nối dữ liệu sau này:
        // 1. Bind từ ViewModel
        // 2. Hoặc truyền model từ ngoài vào rồi map ra các DependencyProperty
        */
    }
}
