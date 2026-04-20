using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Model.Word;
using Royal_Blueberry_Dictionary.Service;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class GameViewModel : ObservableObject
    {
        private readonly GameLogService _gameLogService;

        #region Mạng Lưới Properties (Được MVVM Toolkit Tự Động Sinh Code)

        [ObservableProperty]
        private List<WordEntry> _flashcards = new List<WordEntry>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentCard))]
        [NotifyPropertyChangedFor(nameof(ProgressText))]
        [NotifyPropertyChangedFor(nameof(CanGoBack))]
        [NotifyPropertyChangedFor(nameof(IsLastCard))]
        [NotifyPropertyChangedFor(nameof(NextButtonText))]
        private int _currentCardIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressText))]
        [NotifyPropertyChangedFor(nameof(IsLastCard))]
        [NotifyPropertyChangedFor(nameof(NextButtonText))]
        private int _totalCards;

        [ObservableProperty]
        private bool _isFlipped;

        [ObservableProperty]
        private bool _isAnimating;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSkippedCards))]
        [NotifyPropertyChangedFor(nameof(SkipTrackerMessage))]
        private ObservableCollection<int> _skippedCards = new ObservableCollection<int>();

        [ObservableProperty]
        private List<int> _knownCards = new List<int>();

        #endregion

        #region UI Computed Properties

        public WordEntry CurrentCard => (Flashcards?.Count > 0 && CurrentCardIndex >= 0 && CurrentCardIndex < Flashcards.Count) ? Flashcards[CurrentCardIndex] : null;
        public string ProgressText => $"{CurrentCardIndex + 1}/{TotalCards}";
        public bool CanGoBack => CurrentCardIndex > 0;
        public bool IsLastCard => TotalCards > 0 && CurrentCardIndex >= TotalCards - 1;
        public string NextButtonText => IsLastCard ? "Finish ✓" : "Next (Known) ▶";
        public bool HasSkippedCards => SkippedCards?.Count > 0;
        public string SkipTrackerMessage => HasSkippedCards ? "🚩 Skipped:" : "✅ No skipped cards yet!";

        public string DataSourceName { get; set; }
        private GameSession _currentSession;

        #endregion

        public GameViewModel(GameLogService gameLogService)
        {
            _gameLogService = gameLogService;

            // Trigger update UI khi list skipped thay đổi
            SkippedCards.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasSkippedCards));
                OnPropertyChanged(nameof(SkipTrackerMessage));
            };
        }

        #region Logic Game
        public void StartGame(List<WordEntry> cards, string dataSource, string dataSourceName)
        {
            Flashcards = cards ?? new List<WordEntry>();
            TotalCards = Flashcards.Count;
            CurrentCardIndex = 0;
            IsFlipped = false;
            IsAnimating = false;
            SkippedCards.Clear();
            KnownCards.Clear();
            DataSourceName = dataSourceName;

            _currentSession = new GameSession
            {
                StartTime = DateTime.Now,
                DataSource = dataSource,
                DataSourceName = dataSourceName,
                TotalCards = TotalCards
            };

            OnPropertyChanged(nameof(CurrentCard));
        }

        public GameCompletionData CompleteGame()
        {
            int knownCount = KnownCards.Count;
            int unknownCount = SkippedCards.Count;
            int reviewedCount = TotalCards - (knownCount + unknownCount);
            knownCount += reviewedCount; // Các thẻ đã xem mà không skip mặc định là known

            int percentage = TotalCards > 0 ? (int)Math.Round((double)knownCount / TotalCards * 100) : 0;

            if (_currentSession != null)
            {
                _currentSession.EndTime = DateTime.Now;
                _currentSession.Duration = _currentSession.EndTime - _currentSession.StartTime;
                _currentSession.KnownCards = knownCount;
                _currentSession.UnknownCards = unknownCount;
                _currentSession.AccuracyPercentage = percentage;
                _currentSession.SkippedCardIndices = SkippedCards.ToList();
                _currentSession.SkippedWords = SkippedCards.Select(idx => Flashcards[idx].Word).ToList();

                _gameLogService.AddSession(_currentSession);
            }

            return new GameCompletionData
            {
                Percentage = percentage,
                KnownCount = knownCount,
                UnknownCount = unknownCount,
                TotalCount = TotalCards,
                SkippedIndices = SkippedCards.ToList()
            };
        }

        public void NextCard()
        {
            if (!KnownCards.Contains(CurrentCardIndex) && !SkippedCards.Contains(CurrentCardIndex))
                KnownCards.Add(CurrentCardIndex);

            SkippedCards.Remove(CurrentCardIndex);
            GoToCard(CurrentCardIndex + 1);
        }

        public void PreviousCard() => GoToCard(CurrentCardIndex - 1);

        public void SkipCurrentCard()
        {
            if (!SkippedCards.Contains(CurrentCardIndex)) SkippedCards.Add(CurrentCardIndex);
            KnownCards.Remove(CurrentCardIndex);
            GoToCard(CurrentCardIndex + 1);
        }

        public void GoToCard(int index)
        {
            if (index >= 0 && index < TotalCards)
            {
                CurrentCardIndex = index;
                IsFlipped = false;
            }
        }

        public void GoToFirstSkipped()
        {
            if (HasSkippedCards) GoToCard(SkippedCards.OrderBy(x => x).First());
        }

        public void RestartGame()
        {
            CurrentCardIndex = 0;
            SkippedCards.Clear();
            KnownCards.Clear();
            IsFlipped = false;
            IsAnimating = false;
            _currentSession.StartTime = DateTime.Now; // Reset time
        }
        #endregion
    }
}