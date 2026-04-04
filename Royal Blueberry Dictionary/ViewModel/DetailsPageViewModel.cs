using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using INavigationAware = Royal_Blueberry_Dictionary.Service.INavigationAware;

namespace BlueBerryDictionary.ViewModels
{
    public partial class DetailsPageViewModel : ObservableObject, INavigationAware
    {
        private readonly NavigationService _navigationService;
        private MediaPlayer _mediaPlayer = new();

        // ── Observable Properties ─────────────────────────────────────
        [ObservableProperty] private WordDetail _wordDetail;
        [ObservableProperty] private string _wordTitle;
        [ObservableProperty] private string _phoneticUs;
        [ObservableProperty] private string _phoneticUk;
        [ObservableProperty] private string _usAudioUrl;
        [ObservableProperty] private string _ukAudioUrl;
        [ObservableProperty] private bool _isFavorite;
        private ObservableCollection<Meaning> meanings; 
        public DetailsPageViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        // ── INavigationAware ──────────────────────────────────────────
        public void OnNavigatedTo(object parameter)
        {
            if (parameter is not WordDetail detail) return;

            WordDetail = detail;
            WordTitle = CapitalizeFirstLetter(detail.Word);
            PhoneticUs = detail.Phonetic ?? "/n/a/";
            PhoneticUk = detail.Phonetic ?? "/n/a/";
            UsAudioUrl = detail.AudioUs;
            UkAudioUrl = detail.AudioUk;
            meanings = new ObservableCollection<Meaning>(detail.Meanings ?? new List<Meaning>());          
        }

        public void OnNavigatedFrom()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
        }

        // ── Commands ──────────────────────────────────────────────────
        [RelayCommand]
        private void PlayAudioUs() => PlayAudio(UsAudioUrl, "US");

        [RelayCommand]
        private void PlayAudioUk() => PlayAudio(UkAudioUrl, "UK");

        [RelayCommand]
        private void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
            // TODO: inject VocabService sau khi implement
        }

        [RelayCommand]
        private void SaveWord()
        {
            // TODO: inject VocabService + MeaningSelectorDialog sau
        }

        [RelayCommand]
        private void ShareWord()
        {
            if (WordDetail?.Word == null) return;
            Clipboard.SetText(WordDetail.Word);
            MessageBox.Show("Word copied successfully");
        }

        [RelayCommand]
        private async Task SearchRelatedWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;
            // Navigate lại DetailsPage với từ mới
            // SearchService cần inject nếu muốn fetch
            _navigationService.NavigateTo<DetailsPage, DetailsPageViewModel>(word);
        }

        // ── Private helpers ───────────────────────────────────────────
        private void PlayAudio(string url, string accent)
        {
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show($"No audio available ({accent})", "Notification");
                return;
            }
            _mediaPlayer.Stop();
            _mediaPlayer.Open(new Uri(url, UriKind.Absolute));
            _mediaPlayer.Play();
        }

        private static string CapitalizeFirstLetter(string text) =>
            string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0]) + text[1..];
    }
}