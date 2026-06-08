using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Royal_Blueberry_Dictionary.ViewModel;
using Royal_Blueberry_Dictionary.View.Dialogs;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class GamePage : Page
    {
        private GameViewModel ViewModel => (GameViewModel)DataContext;

        public GamePage()
        {
            InitializeComponent();
        }

        private void GameCard_Click(object sender, MouseButtonEventArgs e)
        {
            var settingsDialog = new GameSettingsDialog { Owner = Window.GetWindow(this) };

            if (settingsDialog.ShowDialog() == true)
            {
                var settings = settingsDialog.GameSettings;
                ViewModel.StartGame(settings.Flashcards, settings.DataSource, settings.DataSourceName);

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
            if (ViewModel.IsAnimating) return;
            ViewModel.IsAnimating = true;
            ((Storyboard)FindResource(ViewModel.IsFlipped ? "FlipToFrontPhase1" : "FlipToBackPhase1")).Begin(this);
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
            ViewModel.IsAnimating = false;
            ViewModel.IsFlipped = !ViewModel.IsFlipped;
        }

        private void PreviousCard_Click(object sender, RoutedEventArgs e) => ViewModel.PreviousCard();

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsLastCard)
            {
                if (!ViewModel.KnownCards.Contains(ViewModel.CurrentCardIndex) && !ViewModel.SkippedCards.Contains(ViewModel.CurrentCardIndex))
                    ViewModel.KnownCards.Add(ViewModel.CurrentCardIndex);
                ShowCompletionDialog();
            }
            else ViewModel.NextCard();
        }

        private void SkipCard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SkipCurrentCard();
            if (ViewModel.IsLastCard) ShowCompletionDialog();
        }

        private void ReviewSkipped_Click(object sender, RoutedEventArgs e) => ViewModel.GoToFirstSkipped();
        private void SkipNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index) ViewModel.GoToCard(index);
        }

        private void ShowCompletionDialog()
        {
            var data = ViewModel.CompleteGame();
            if (data == null) return;

            var dialog = new GameCompletionDialog { Owner = Window.GetWindow(this) };
            dialog.SetCompletionData(data.Percentage, data.KnownCount, data.UnknownCount, data.TotalCount, data.SkippedIndices);

            if (dialog.ShowDialog() == true)
            {
                if (dialog.UserAction == GameCompletionDialog.CompletionAction.Restart)
                    ViewModel.RestartGame();
                else if (dialog.UserAction == GameCompletionDialog.CompletionAction.ReviewSkipped)
                    ViewModel.GoToCard(dialog.SelectedCardIndex ?? ViewModel.SkippedCards[0]);
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