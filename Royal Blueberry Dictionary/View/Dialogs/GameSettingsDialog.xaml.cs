using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Word;
using Royal_Blueberry_Dictionary.Repository.Interface; // Nơi chứa IWordEntryRepository và ITagRepository của bạn

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class GameSettingsDialog : Window
    {
        private readonly IWordEntryRepository _wordRepo;
        private readonly ITagRepository _tagRepo;

        private string _selectedDataSource = "All";
        private Tag _selectedTag = null;
        private int _selectedCardCount = 10;

        public GameSettings GameSettings { get; private set; }

        public GameSettingsDialog()
        {
            InitializeComponent();

            // Inject repositories từ App
            _wordRepo = App.serviceProvider.GetService<IWordEntryRepository>();
            _tagRepo = App.serviceProvider.GetService<ITagRepository>();

            LoadDataSourceOptions();
        }

        private async void LoadDataSourceOptions()
        {
            DataSourceOptions.Children.Clear();
            var allButtons = new List<Button>();

            allButtons.Add(CreateOptionButton("📚 All Words", "All"));
            // allButtons.Add(CreateOptionButton("⭐ Favorites", "Favorites")); (Thêm nếu repo của bạn có hàm GetFavorites)

            var tags = await _tagRepo.GetAllTagsAsync();
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    allButtons.Add(CreateOptionButton($"🏷️ {tag.Name}", tag.Id.ToString()));
                }
            }

            for (int i = 0; i < allButtons.Count; i++)
            {
                if (i == 0) allButtons[i].Style = (Style)FindResource("PopupItemFirstStyle");
                if (i == 1 && allButtons.Count > 1) DataSourceOptions.Children.Add(new Separator { Margin = new Thickness(0) });
                if (i == allButtons.Count - 1) allButtons[i].Style = (Style)FindResource("PopupItemLastStyle");

                DataSourceOptions.Children.Add(allButtons[i]);
            }
            UpdateAvailableCardsInfo();
        }

        private Button CreateOptionButton(string content, string tagId)
        {
            var btn = new Button { Content = content, Tag = tagId, Style = (Style)FindResource("PopupItemStyle") };
            btn.Click += DataSourceOption_Click;
            return btn;
        }

        private void BtnDataSource_Click(object sender, RoutedEventArgs e)
        {
            PopupDataSource.IsOpen = !PopupDataSource.IsOpen;
            if (PopupDataSource.IsOpen) PopupDataSource.Width = BtnDataSource.ActualWidth;
        }

        private async void DataSourceOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                TxtSelectedSource.Text = btn.Content.ToString();
                _selectedDataSource = btn.Tag.ToString();
                PopupDataSource.IsOpen = false;
                UpdateAvailableCardsInfo();
            }
        }

        private async void UpdateAvailableCardsInfo()
        {
            var words = await GetWordsFromSourceAsync();
            int availableCount = words?.Count ?? 0;
            TxtAvailableCards.Text = $"{availableCount} available words";

            if (_selectedCardCount > availableCount)
            {
                _selectedCardCount = Math.Max(1, availableCount);
                TxtSelectedCount.Text = $"{_selectedCardCount} cards";
            }
        }

        private async System.Threading.Tasks.Task<List<WordEntry>> GetWordsFromSourceAsync()
        {
            string currentUserId = "ID_USER";
            if (_selectedDataSource == "All")
            {
                return await _wordRepo.GetAllAsync(currentUserId);
            }
            else
            {
                if (int.TryParse(_selectedDataSource, out int tagId))
                {
                    // Lấy các từ thuộc TagId đó (tuỳ hàm bạn định nghĩa trong Repo)
                    // Đây là ví dụ nếu bạn chưa có hàm, hãy tự map theo logic DB của bạn.
                    var allWords = await _wordRepo.GetAllAsync(currentUserId);
                    // Giả sử logic của bạn cần join bảng, bạn thực hiện qua repo. Tạm return rỗng nếu chưa có.
                    return allWords;
                }
                return new List<WordEntry>();
            }
        }

        private void BtnCardCount_Click(object sender, RoutedEventArgs e)
        {
            PopupCardCount.IsOpen = !PopupCardCount.IsOpen;
            if (PopupCardCount.IsOpen) PopupCardCount.Width = BtnCardCount.ActualWidth;
        }

        private void CardCountOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                TxtSelectedCount.Text = btn.Content.ToString();
                _selectedCardCount = int.Parse(btn.Tag.ToString());
                PopupCardCount.IsOpen = false;
            }
        }

        private async void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var sourceWords = await GetWordsFromSourceAsync();
            if (sourceWords == null || sourceWords.Count == 0)
            {
                MessageBox.Show("No words to study! Please select a different source.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var random = new Random();
            var flashcards = sourceWords.OrderBy(x => random.Next()).Take(Math.Min(_selectedCardCount, sourceWords.Count)).ToList();

            GameSettings = new GameSettings
            {
                DataSource = _selectedDataSource,
                DataSourceName = TxtSelectedSource.Text,
                CardCount = _selectedCardCount,
                Flashcards = flashcards
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    }
}