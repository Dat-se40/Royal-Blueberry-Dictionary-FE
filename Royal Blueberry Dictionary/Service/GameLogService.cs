using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service.ApiClient;

namespace Royal_Blueberry_Dictionary.Service
{
    public class GameLogService
    {
        private readonly IBackendApiClient _api;
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private GameLog _currentLog;

        public GameLogService(IBackendApiClient api)
        {
            _api = api;
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RoyalBlueberryDictionary",
                "GameLogs");
            _logFilePath = Path.Combine(_logDirectory, "GameLog.json");
            LoadLog();
        }

        public void AddSession(GameSession session)
        {
            _currentLog.Sessions.Add(session);
            _currentLog.TotalGamesPlayed++;
            _currentLog.TotalCardsStudied += session.TotalCards;
            SaveLog();
            _ = SyncSessionToApiAsync(session);
        }

        public async Task<GameLogOverview> GetOverviewAsync(int recentLimit = 20)
        {
            if (CanUseApi())
            {
                try
                {
                    var summary = await _api.GetAsync<GameLogSummaryResponse>("game-logs/summary");
                    var sessions = await _api.GetAsync<List<GameSession>>($"game-logs/sessions?limit={recentLimit}");

                    if (summary != null && sessions != null)
                    {
                        return new GameLogOverview
                        {
                            TotalGamesPlayed = summary.TotalGamesPlayed,
                            TotalCardsStudied = summary.TotalCardsStudied,
                            AverageAccuracy = summary.AverageAccuracy,
                            TotalStudyTime = TimeSpan.FromSeconds(summary.TotalStudyTimeSeconds),
                            RecentSessions = sessions,
                            IsFromApi = true
                        };
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Game log API overview failed, using local cache: {ex.Message}");
                }
            }

            return BuildLocalOverview(recentLimit);
        }

        public async Task ClearAllSessionsAsync()
        {
            if (CanUseApi())
            {
                try
                {
                    await _api.DeleteAsync("game-logs/sessions");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Game log API clear failed: {ex.Message}");
                }
            }

            _currentLog = new GameLog();
            SaveLog();
        }

        public List<GameSession> GetRecentSessions(int count = 10) =>
            _currentLog.Sessions.OrderByDescending(s => s.StartTime).Take(count).ToList();

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

        private bool CanUseApi() => TokenManager.HasStoredTokens();

        private async Task SyncSessionToApiAsync(GameSession session)
        {
            if (!CanUseApi()) return;

            try
            {
                var request = new SaveGameSessionRequest
                {
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    DataSource = session.DataSource,
                    DataSourceName = session.DataSourceName,
                    TotalCards = session.TotalCards,
                    KnownCards = session.KnownCards,
                    UnknownCards = session.UnknownCards,
                    AccuracyPercentage = session.AccuracyPercentage,
                    DurationSeconds = (long)session.Duration.TotalSeconds,
                    SkippedCardIndices = session.SkippedCardIndices,
                    SkippedWords = session.SkippedWords
                };

                var saved = await _api.PostAsync<GameSession>("game-logs/sessions", request);
                if (!string.IsNullOrWhiteSpace(saved?.Id))
                {
                    session.Id = saved.Id;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Game log API sync failed: {ex.Message}");
            }
        }

        private GameLogOverview BuildLocalOverview(int recentLimit)
        {
            return new GameLogOverview
            {
                TotalGamesPlayed = GetTotalGamesPlayed(),
                TotalCardsStudied = GetTotalCardsStudied(),
                AverageAccuracy = GetAverageAccuracy(),
                TotalStudyTime = GetTotalStudyTime(),
                RecentSessions = GetRecentSessions(recentLimit),
                IsFromApi = false
            };
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
                Debug.WriteLine($"Error saving game log: {ex.Message}");
            }
        }
    }
}
