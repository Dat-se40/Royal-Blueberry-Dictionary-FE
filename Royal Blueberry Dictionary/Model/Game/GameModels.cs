using System;
using System.Collections.Generic;
using Royal_Blueberry_Dictionary.Model.Word; // Import WordEntry

namespace Royal_Blueberry_Dictionary.Model
{
    public class GameSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int TotalCards { get; set; }
        public int KnownCards { get; set; }
        public int UnknownCards { get; set; }
        public double AccuracyPercentage { get; set; }
        public TimeSpan Duration { get; set; }
        public List<int> SkippedCardIndices { get; set; } = new List<int>();
        public List<string> SkippedWords { get; set; } = new List<string>();

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
        public List<int> SkippedIndices { get; set; }
    }
}