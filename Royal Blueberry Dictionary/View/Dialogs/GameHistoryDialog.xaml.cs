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
            _gameLogService = App.serviceProvider.GetService<GameLogService>();
            LoadHistoryData();
        }

        private void LoadHistoryData()
        {
            if (_gameLogService == null) return;

            TxtTotalGames.Text = _gameLogService.GetTotalGamesPlayed().ToString();
            TxtTotalCards.Text = _gameLogService.GetTotalCardsStudied().ToString();
            TxtAvgAccuracy.Text = $"{_gameLogService.GetAverageAccuracy():F1}%";

            var ts = _gameLogService.GetTotalStudyTime();
            TxtTotalTime.Text = ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}h {ts.Minutes}m" : (ts.TotalMinutes >= 1 ? $"{ts.Minutes}m {ts.Seconds}s" : $"{ts.Seconds}s");

            HistoryList.ItemsSource = _gameLogService.GetRecentSessions(20);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all history?\nThis action cannot be undone.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _gameLogService.ClearAllSessions();
                LoadHistoryData();
                MessageBox.Show("All history deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}