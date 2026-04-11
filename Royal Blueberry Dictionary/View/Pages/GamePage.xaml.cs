using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Royal_Blueberry_Dictionary.ViewModel;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class GamePage : Page
    {
        private GameViewModel _viewModel;

        public GamePage()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            this.DataContext = _viewModel;
        }

        private void GameCard_Click(object sender, MouseButtonEventArgs e)
        {
            // Tạm thời dùng dữ liệu giả để test, bạn sẽ thay bằng logic lấy từ WordService sau
            _viewModel.StartGame(new System.Collections.Generic.List<Model.WordEntry>(), "Tất cả từ");

            GameSelectionPanel.Visibility = Visibility.Collapsed;
            GamePlayPanel.Visibility = Visibility.Visible;
        }

        private void FlipCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsAnimating) return;
            _viewModel.IsAnimating = true;

            string sbName = !_viewModel.IsFlipped ? "FlipToBackPhase1" : "FlipToFrontPhase1";
            ((Storyboard)FindResource(sbName)).Begin();
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