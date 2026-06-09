using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Service;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class GameHistoryDialog : Window
    {
        private readonly GameLogService _gameLogService;

        public GameHistoryDialog()
        {
            InitializeComponent();
            _gameLogService = App.serviceProvider.GetRequiredService<GameLogService>();
            Loaded += async (_, _) => await LoadHistoryDataAsync();
        }

        private async Task LoadHistoryDataAsync()
        {
            if (_gameLogService == null) return;

            try
            {
                var overview = await _gameLogService.GetOverviewAsync(20);

                TxtTotalGames.Text = overview.TotalGamesPlayed.ToString();
                TxtTotalCards.Text = overview.TotalCardsStudied.ToString();
                TxtAvgAccuracy.Text = $"{overview.AverageAccuracy:F1}%";
                TxtTotalTime.Text = FormatDuration(overview.TotalStudyTime);
                HistoryList.ItemsSource = overview.RecentSessions;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load training history.\n{ex.Message}",
                    "History",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private static string FormatDuration(TimeSpan ts)
        {
            if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
            if (ts.TotalMinutes >= 1) return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }

        private async void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                    "Are you sure you want to delete all history?\nThis action cannot be undone.",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            await _gameLogService.ClearAllSessionsAsync();
            await LoadHistoryDataAsync();
            MessageBox.Show("All history deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
