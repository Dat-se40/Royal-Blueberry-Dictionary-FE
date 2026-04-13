using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Royal_Blueberry_Dictionary.Model;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class GameViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<WordEntry> _flashcards = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressText))]
        [NotifyPropertyChangedFor(nameof(CanGoBack))]
        [NotifyPropertyChangedFor(nameof(IsLastCard))]
        [NotifyPropertyChangedFor(nameof(NextButtonText))]
        [NotifyPropertyChangedFor(nameof(CurrentCard))]
        private int _currentCardIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressText))]
        private int _totalCards;

        [ObservableProperty]
        private bool _isFlipped;

        [ObservableProperty]
        private bool _isAnimating;

        [ObservableProperty]
        private ObservableCollection<int> _skippedCards = new();

        [ObservableProperty]
        private List<int> _knownCards = new();

        public WordEntry? CurrentCard =>
            (Flashcards != null && Flashcards.Count > 0 && CurrentCardIndex < Flashcards.Count)
            ? Flashcards[CurrentCardIndex] : null;

        public string ProgressText => $"{CurrentCardIndex + 1}/{TotalCards}";
        public bool CanGoBack => CurrentCardIndex > 0;
        public bool IsLastCard => CurrentCardIndex >= TotalCards - 1;
        public string NextButtonText => IsLastCard ? "Finish ✓" : "Next (Known) ▶";
        public bool HasSkippedCards => SkippedCards.Count > 0;
        public string SkipTrackerMessage => HasSkippedCards ? "🚩 Skipped:" : "✅ No skipped cards yet!";

        public string DataSourceName { get; set; } = string.Empty;

        public void StartGame(List<WordEntry> cards, string dataSourceName)
        {
            Flashcards = cards;
            TotalCards = cards.Count;
            CurrentCardIndex = 0;
            IsFlipped = false;
            IsAnimating = false;
            SkippedCards.Clear();
            KnownCards.Clear();
            DataSourceName = dataSourceName;
        }

        public void GoToCard(int index)
        {
            if (index >= 0 && index < TotalCards)
            {
                CurrentCardIndex = index;
                IsFlipped = false;
            }
        }

        // PHƯƠNG THỨC NÀY CẦN PHẢI CÓ ĐỂ SỬA LỖI CỦA BẠN
        public void PreviousCard()
        {
            if (CanGoBack)
            {
                GoToCard(CurrentCardIndex - 1);
            }
        }

        public void NextCard()
        {
            if (!KnownCards.Contains(CurrentCardIndex) && !SkippedCards.Contains(CurrentCardIndex))
                KnownCards.Add(CurrentCardIndex);

            if (SkippedCards.Contains(CurrentCardIndex))
                SkippedCards.Remove(CurrentCardIndex);

            GoToCard(CurrentCardIndex + 1);
        }

        public void SkipCurrentCard()
        {
            if (!SkippedCards.Contains(CurrentCardIndex))
                SkippedCards.Add(CurrentCardIndex);

            KnownCards.Remove(CurrentCardIndex);
            GoToCard(CurrentCardIndex + 1);
        }

        public void GoToFirstSkipped()
        {
            if (SkippedCards.Count > 0)
                GoToCard(SkippedCards.OrderBy(x => x).First());
        }
    }
}