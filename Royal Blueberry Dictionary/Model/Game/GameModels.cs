using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Royal_Blueberry_Dictionary.Model.Word;

namespace Royal_Blueberry_Dictionary.Model
{
    public class GameSession
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("dataSource")]
        public string DataSource { get; set; } = string.Empty;

        [JsonPropertyName("dataSourceName")]
        public string DataSourceName { get; set; } = string.Empty;

        [JsonPropertyName("totalCards")]
        public int TotalCards { get; set; }

        [JsonPropertyName("knownCards")]
        public int KnownCards { get; set; }

        [JsonPropertyName("unknownCards")]
        public int UnknownCards { get; set; }

        [JsonPropertyName("accuracyPercentage")]
        public double AccuracyPercentage { get; set; }

        public TimeSpan Duration { get; set; }

        [JsonPropertyName("durationSeconds")]
        public long DurationSeconds
        {
            get => (long)Duration.TotalSeconds;
            set => Duration = TimeSpan.FromSeconds(value);
        }

        [JsonPropertyName("skippedCardIndices")]
        public List<int> SkippedCardIndices { get; set; } = new List<int>();

        [JsonPropertyName("skippedWords")]
        public List<string> SkippedWords { get; set; } = new List<string>();

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        // Thuộc tính hỗ trợ UI hiển thị thời gian
        public string DurationText
        {
            get
            {
                if (Duration.TotalHours >= 1) return $"{(int)Duration.TotalHours}h {Duration.Minutes}m";
                if (Duration.TotalMinutes >= 1) return $"{Duration.Minutes}m {Duration.Seconds}s";
                return $"{Duration.Seconds}s";
            }
        }
    }

    public class GameLog
    {
        public List<GameSession> Sessions { get; set; } = new List<GameSession>();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public int TotalGamesPlayed { get; set; } = 0;
        public int TotalCardsStudied { get; set; } = 0;
    }

    // Dùng để truyền dữ liệu từ Settings Dialog sang ViewModel
    public class GameSettings
    {
        public string DataSource { get; set; }
        public string DataSourceName { get; set; }
        public Tag SelectedTag { get; set; }
        public int CardCount { get; set; }
        public List<WordEntry> Flashcards { get; set; } // Dùng WordEntry của Model mới
    }

    // Dùng để truyền dữ liệu khi hoàn thành game
    public class GameCompletionData
    {
        public int Percentage { get; set; }
        public int KnownCount { get; set; }
        public int UnknownCount { get; set; }
        public int TotalCount { get; set; }
        public List<int> SkippedIndices { get; set; } = new List<int>();
    }

    public class SaveGameSessionRequest
    {
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("dataSource")]
        public string DataSource { get; set; } = string.Empty;

        [JsonPropertyName("dataSourceName")]
        public string DataSourceName { get; set; } = string.Empty;

        [JsonPropertyName("totalCards")]
        public int TotalCards { get; set; }

        [JsonPropertyName("knownCards")]
        public int KnownCards { get; set; }

        [JsonPropertyName("unknownCards")]
        public int UnknownCards { get; set; }

        [JsonPropertyName("accuracyPercentage")]
        public double AccuracyPercentage { get; set; }

        [JsonPropertyName("durationSeconds")]
        public long DurationSeconds { get; set; }

        [JsonPropertyName("skippedCardIndices")]
        public List<int> SkippedCardIndices { get; set; } = new List<int>();

        [JsonPropertyName("skippedWords")]
        public List<string> SkippedWords { get; set; } = new List<string>();
    }

    public class GameLogSummaryResponse
    {
        [JsonPropertyName("totalGamesPlayed")]
        public int TotalGamesPlayed { get; set; }

        [JsonPropertyName("totalCardsStudied")]
        public int TotalCardsStudied { get; set; }

        [JsonPropertyName("averageAccuracy")]
        public double AverageAccuracy { get; set; }

        [JsonPropertyName("totalStudyTimeSeconds")]
        public long TotalStudyTimeSeconds { get; set; }
    }

    public class GameLogOverview
    {
        public int TotalGamesPlayed { get; set; }
        public int TotalCardsStudied { get; set; }
        public double AverageAccuracy { get; set; }
        public TimeSpan TotalStudyTime { get; set; }
        public List<GameSession> RecentSessions { get; set; } = new List<GameSession>();
        public bool IsFromApi { get; set; }
    }
}