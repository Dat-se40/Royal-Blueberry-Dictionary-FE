using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.ViewModel;
using Royal_Blueberry_Dictionary.View.Dialogs;
using Royal_Blueberry_Dictionary.Service;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class GamePage : Page
    {
        private readonly GameViewModel _viewModel;

        public GamePage()
        {
            InitializeComponent();
            // Lấy ViewModel từ cơ chế Dependency Injection thay vì khởi tạo chay bằng "new GameViewModel()"
            _viewModel = App.serviceProvider.GetRequiredService<GameViewModel>();
            DataContext = _viewModel;
        }

        private void GameCard_Click(object sender, MouseButtonEventArgs e)
        {
            var settingsDialog = new GameSettingsDialog { Owner = Window.GetWindow(this) };

            if (settingsDialog.ShowDialog() == true)
            {
                var settings = settingsDialog.GameSettings;
                _viewModel.StartGame(settings.Flashcards, settings.DataSource, settings.DataSourceName);

                GameSelectionPanel.Visibility = Visibility.Collapsed;
                GamePlayPanel.Visibility = Visibility.Visible;
            }
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            new GameHistoryDialog { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        private void FlipCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsAnimating) return;
            _viewModel.IsAnimating = true;
            ((Storyboard)FindResource(_viewModel.IsFlipped ? "FlipToFrontPhase1" : "FlipToBackPhase1")).Begin(this);
        }

        private void FlipToBackPhase1_Completed(object sender, EventArgs e)
        {
            CardFront.Visibility = Visibility.Collapsed;
            CardBack.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("FlipToBackPhase2")).Begin(this);
        }

        private void FlipToFrontPhase1_Completed(object sender, EventArgs e)
        {
            CardBack.Visibility = Visibility.Collapsed;
            CardFront.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("FlipToFrontPhase2")).Begin(this);
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            _viewModel.IsAnimating = false;
            _viewModel.IsFlipped = !_viewModel.IsFlipped;
        }

        private void PreviousCard_Click(object sender, RoutedEventArgs e) => _viewModel.PreviousCard();

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsLastCard)
            {
                if (!_viewModel.KnownCards.Contains(_viewModel.CurrentCardIndex) && !_viewModel.SkippedCards.Contains(_viewModel.CurrentCardIndex))
                    _viewModel.KnownCards.Add(_viewModel.CurrentCardIndex);
                ShowCompletionDialog();
            }
            else _viewModel.NextCard();
        }

        private void SkipCard_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SkipCurrentCard();
            if (_viewModel.IsLastCard) ShowCompletionDialog();
        }

        private void ReviewSkipped_Click(object sender, RoutedEventArgs e) => _viewModel.GoToFirstSkipped();
        private void SkipNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index) _viewModel.GoToCard(index);
        }

        private void ShowCompletionDialog()
        {
            var data = _viewModel.CompleteGame();
            if (data == null) return;

            var dialog = new GameCompletionDialog { Owner = Window.GetWindow(this) };
            dialog.SetCompletionData(data.Percentage, data.KnownCount, data.UnknownCount, data.TotalCount, data.SkippedIndices);

            if (dialog.ShowDialog() == true)
            {
                if (dialog.UserAction == GameCompletionDialog.CompletionAction.Restart)
                    _viewModel.RestartGame();
                else if (dialog.UserAction == GameCompletionDialog.CompletionAction.ReviewSkipped)
                    _viewModel.GoToCard(dialog.SelectedCardIndex ?? _viewModel.SkippedCards[0]);
            }
            else
            {
                GamePlayPanel.Visibility = Visibility.Collapsed;
                GameSelectionPanel.Visibility = Visibility.Visible;
            }
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure? Progress will be lost.", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GamePlayPanel.Visibility = Visibility.Collapsed;
                GameSelectionPanel.Visibility = Visibility.Visible;
            }
        }
    }
}