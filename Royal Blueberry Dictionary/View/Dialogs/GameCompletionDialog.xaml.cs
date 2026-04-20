using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class GameCompletionDialog : Window
    {
        public enum CompletionAction { Close, Restart, ReviewSkipped }
        public CompletionAction UserAction { get; private set; } = CompletionAction.Close;
        public int? SelectedCardIndex { get; private set; }

        public GameCompletionDialog() { InitializeComponent(); }

        public void SetCompletionData(int percentage, int knownCount, int unknownCount, int totalCount, List<int> skippedIndices)
        {
            TxtPercentage.Text = $"{percentage}%";
            TxtKnownCount.Text = $"{knownCount} cards ({percentage}%)";
            TxtUnknownCount.Text = $"{unknownCount} cards ({100 - percentage}%)";
            TxtTotalCount.Text = $"{totalCount} cards";

            if (skippedIndices != null && skippedIndices.Count > 0)
            {
                SkippedListContainer.Visibility = Visibility.Visible;
                SkippedNumbersList.ItemsSource = skippedIndices.Select(i => i + 1).ToList();
                Actions2Buttons.Visibility = Visibility.Collapsed;
                Actions3Buttons.Visibility = Visibility.Visible;
            }
            else
            {
                SkippedListContainer.Visibility = Visibility.Collapsed;
                Actions3Buttons.Visibility = Visibility.Collapsed;
                Actions2Buttons.Visibility = Visibility.Visible;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) { UserAction = CompletionAction.Close; DialogResult = false; Close(); }
        private void Restart_Click(object sender, RoutedEventArgs e) { UserAction = CompletionAction.Restart; DialogResult = true; Close(); }
        private void ReviewSkipped_Click(object sender, RoutedEventArgs e) { UserAction = CompletionAction.ReviewSkipped; DialogResult = true; Close(); }
        private void SkipNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int displayNumber)
            {
                SelectedCardIndex = displayNumber - 1;
                UserAction = CompletionAction.ReviewSkipped;
                DialogResult = true;
                Close();
            }
        }
    }
}