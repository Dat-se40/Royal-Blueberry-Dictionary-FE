using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.Model;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class GamePage : Page
    {
        private readonly GameViewModel _viewModel;
        private readonly Service.WordService _wordService;

        public GamePage(GameViewModel viewModel, Service.WordService wordService)
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            _wordService = wordService;
            this.DataContext = _viewModel;
        }

        private async void GameCard_Click(object sender, MouseButtonEventArgs e)
        {
            var userWords = await _wordService.GetAllWordsAsync();
            if (userWords == null || userWords.Count == 0)
            {
                userWords = new List<Model.WordEntry>
                {
                    new Model.WordEntry { Word = "Blueberry", Phonetic = "/ˈbluː.bər.i/", PartOfSpeech = "Noun", Definition = "Quả việt quất", Example = "Blueberries are healthy." },
                    new Model.WordEntry { Word = "Dictionary", Phonetic = "/ˈdɪk.ʃən.ər.i/", PartOfSpeech = "Noun", Definition = "Từ điển", Example = "I use a dictionary." }
                };
            }
            _viewModel.StartGame(userWords, "My Vocabulary");

            GameSelectionPanel.Visibility = Visibility.Collapsed;
            GamePlayPanel.Visibility = Visibility.Visible;
        }

        private void FlipCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsAnimating) return;
            _viewModel.IsAnimating = true;

            string sbName = !_viewModel.IsFlipped ? "FlipToBackPhase1" : "FlipToFrontPhase1";
            var sb = (Storyboard)FindResource(sbName);
            if (sb != null) sb.Begin();
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsLastCard)
            {
                MessageBox.Show("Bạn đã hoàn thành lượt học!");
                GamePlayPanel.Visibility = Visibility.Collapsed;
                GameSelectionPanel.Visibility = Visibility.Visible;
            }
            else _viewModel.NextCard();
        }

        private void PreviousCard_Click(object sender, RoutedEventArgs e) => _viewModel.PreviousCard();

        private void SkipCard_Click(object sender, RoutedEventArgs e) => _viewModel.SkipCurrentCard();

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            GamePlayPanel.Visibility = Visibility.Collapsed;
            GameSelectionPanel.Visibility = Visibility.Visible;
        }

        // Animation Handlers
        private void AnimationCompleted(object sender, EventArgs e)
        {
            _viewModel.IsAnimating = false;
            _viewModel.IsFlipped = !_viewModel.IsFlipped;
        }

        private void FlipToBackPhase1_Completed(object sender, EventArgs e)
        {
            CardFront.Visibility = Visibility.Collapsed;
            CardBack.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("FlipToBackPhase2")).Begin();
        }

        private void FlipToFrontPhase1_Completed(object sender, EventArgs e)
        {
            CardBack.Visibility = Visibility.Collapsed;
            CardFront.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("FlipToFrontPhase2")).Begin();
        }
    }
}