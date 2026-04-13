using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using INavigationAware = Royal_Blueberry_Dictionary.Service.INavigationAware;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class FavouriteWordsPageViewModel : ObservableObject, INavigationAware
    {
        private readonly WordService _wordService;
        private List<WordEntry> _allFavorites = new();
        private string? _partOfSpeechFilter;

        [ObservableProperty]
        private ObservableCollection<WordEntry> _favoriteWords = new();

        public FavouriteWordsPageViewModel(WordService wordService)
        {
            _wordService = wordService;
        }

        public void OnNavigatedFrom() { }

        public void OnNavigatedTo(object parameter)
        {
            _ = LoadFavoritesAsync();
        }

        private async Task LoadFavoritesAsync()
        {
            await _wordService.CleanUpData(); 
            _allFavorites = await _wordService.GetFavoritedWordsAsync();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<WordEntry> q = _allFavorites;
            if (!string.IsNullOrEmpty(_partOfSpeechFilter))
            {
                q = q.Where(w =>
                    string.Equals(w.PartOfSpeech, _partOfSpeechFilter, StringComparison.OrdinalIgnoreCase));
            }

            FavoriteWords.Clear();
            foreach (var w in q.OrderBy(w => w.Word, StringComparer.OrdinalIgnoreCase))
                FavoriteWords.Add(w);
        }

        [RelayCommand]
        private void ShowAll()
        {
            _partOfSpeechFilter = null;
            ApplyFilter();
        }

        [RelayCommand]
        private void FilterByPartOfSpeech(string? partOfSpeech)
        {
            _partOfSpeechFilter = partOfSpeech;
            ApplyFilter();
        }

        [RelayCommand]
        private async Task ClearAllFavoritesAsync()
        {
            await _wordService.ClearAllFavoritesAsync();
            _allFavorites.Clear();
            FavoriteWords.Clear();
        }

        [RelayCommand]
        public async Task FavoriteAsync(WordEntry? wordEntry)
        {
            if (wordEntry == null) return;
            await _wordService.FavoriteAsync(wordEntry);
            if (!wordEntry.IsFavorited)
            {
                _allFavorites.RemoveAll(w => w.Id == wordEntry.Id);
                FavoriteWords.Remove(wordEntry);
            }
        }
    }
}
