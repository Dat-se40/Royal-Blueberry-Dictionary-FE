using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Royal_Blueberry_Dictionary.Model;

namespace Royal_Blueberry_Dictionary.Service
{
    public class GameLogService
    {
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private GameLog _currentLog;

        public GameLogService()
        {
            // Lưu file log vào thư mục AppData cục bộ của Windows (Tránh lỗi quyền truy cập file)
            _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RoyalBlueberryDictionary", "GameLogs");
            _logFilePath = Path.Combine(_logDirectory, "GameLog.json");
            LoadLog();
        }

        private void LoadLog()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    string json = File.ReadAllText(_logFilePath);
                    _currentLog = JsonSerializer.Deserialize<GameLog>(json) ?? new GameLog();
                }
                else
                {
                    _currentLog = new GameLog();
                }
            }
            catch
            {
                _currentLog = new GameLog();
            }
        }

        private void SaveLog()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                    Directory.CreateDirectory(_logDirectory);

                _currentLog.LastUpdated = DateTime.Now;
                string json = JsonSerializer.Serialize(_currentLog, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_logFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game log: {ex.Message}");
            }
        }

        public void AddSession(GameSession session)
        {
            _currentLog.Sessions.Add(session);
            _currentLog.TotalGamesPlayed++;
            _currentLog.TotalCardsStudied += session.TotalCards;
            SaveLog();
        }

        public List<GameSession> GetRecentSessions(int count = 10)
        {
            return _currentLog.Sessions.OrderByDescending(s => s.StartTime).Take(count).ToList();
        }

        public int GetTotalGamesPlayed() => _currentLog.TotalGamesPlayed;

        public int GetTotalCardsStudied() => _currentLog.TotalCardsStudied;

        public double GetAverageAccuracy()
        {
            if (_currentLog.Sessions.Count == 0) return 0;
            return _currentLog.Sessions.Average(s => s.AccuracyPercentage);
        }

        public TimeSpan GetTotalStudyTime()
        {
            long totalTicks = _currentLog.Sessions.Sum(s => s.Duration.Ticks);
            return TimeSpan.FromTicks(totalTicks);
        }

        public void ClearAllSessions()
        {
            _currentLog = new GameLog();
            SaveLog();
        }
    }
}