using System.Windows.Controls;

namespace Royal_Blueberry_Dictionary.View.User_Control
{
    /// <summary>
    /// FE/UI only for WordItem.
    /// Chỉ dùng để hiển thị dữ liệu lên giao diện.
    /// Các phần click, event, model cũ tạm comment/bỏ qua.
    /// </summary>
    public partial class WordItem : UserControl
    {
        public WordItem()
        {
            InitializeComponent();

            // Chức năng cũ:
            // this.MouseLeftButtonDown += WordItem_MouseLeftButtonDown;
            //
            // Tạm bỏ vì đây là event tương tác chức năng.
            // Bản hiện tại chỉ giữ UI/FE.
        }

        // =========================
        // UI Display Methods
        // =========================

        public void SetDisplay(string word, string phonetic, string meaning)
        {
            tbMainWord.Text = word ?? string.Empty;
            tbPhonetic.Text = phonetic ?? string.Empty;
            tbMeaningText.Text = meaning ?? string.Empty;
        }

        public void SetWord(string word)
        {
            tbMainWord.Text = word ?? string.Empty;
        }

        public void SetPhonetic(string phonetic)
        {
            tbPhonetic.Text = phonetic ?? string.Empty;
        }

        public void SetMeaning(string meaning)
        {
            tbMeaningText.Text = meaning ?? string.Empty;
        }

        public void ClearDisplay()
        {
            tbMainWord.Text = string.Empty;
            tbPhonetic.Text = string.Empty;
            tbMeaningText.Text = string.Empty;
        }

        // =========================
        // Commented Old Logic
        // =========================

        /*
        // Logic cũ:
        // Dùng callback khi click vào item để gửi từ ra ngoài.
        // public Action<string> OnWordClick;
        //
        // Tạm bỏ vì đây là chức năng/event, không thuộc FE-only.
        */

        /*
        // Logic cũ:
        // Hiển thị dữ liệu từ model Word của project cũ.
        // Có phụ thuộc namespace/model cũ: BlueBerryDictionary.Models.Word
        //
        // public void SetUpDisplay(Word mainWord)
        // {
        //     tbMainWord.Text = mainWord.word;
        //     tbPhonetic.Text = mainWord.phonetic;
        //     tbMeaningText.Text = mainWord.meanings[0].definitions[0].definition ?? string.Empty;
        // }
        //
        // Tạm bỏ vì bản này chưa nối model / service / viewmodel.
        */

        /*
        // Logic cũ:
        // Hiển thị dữ liệu từ model ShortenWord của project cũ.
        //
        // public void SetUpDisplay(ShortenWord mainWord)
        // {
        //     tbMainWord.Text = mainWord.word;
        //     tbPhonetic.Text = mainWord.phonetic;
        //     tbMeaningText.Text = mainWord.meaning;
        // }
        //
        // Tạm bỏ vì bản này chỉ giữ giao diện.
        */

        /*
        // Logic cũ:
        // Xử lý khi click vào item.
        //
        // private void WordItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        // {
        //     OnWordClick?.Invoke(tbMainWord.Text);
        // }
        //
        // Tạm bỏ vì đây là event chức năng.
        // Nếu sau này cần:
        // - có thể bind Command từ ViewModel
        // - hoặc expose event ở layer ngoài
        */
    }
}
