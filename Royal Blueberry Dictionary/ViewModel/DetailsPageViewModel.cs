using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using INavigationAware = Royal_Blueberry_Dictionary.Service.INavigationAware;

namespace BlueBerryDictionary.ViewModels
{
    public partial class DetailsPageViewModel : ObservableObject, INavigationAware
    {
        private readonly NavigationService _navigationService;
        private readonly SearchService _searchService;
        private readonly MediaPlayer _mediaPlayer = new();

        [ObservableProperty] private WordDetail? _wordDetail;
        [ObservableProperty] private string _wordTitle = string.Empty;
        [ObservableProperty] private string _phoneticUs = string.Empty;
        [ObservableProperty] private string _phoneticUk = string.Empty;
        [ObservableProperty] private string _usAudioUrl = string.Empty;
        [ObservableProperty] private string _ukAudioUrl = string.Empty;
        [ObservableProperty] private bool _isFavorite;
        [ObservableProperty] private ImageSource? _wordImage;
        [ObservableProperty] private bool _hasWordImage;

        public DetailsPageViewModel(NavigationService navigationService, SearchService searchService)
        {
            _navigationService = navigationService;
            _searchService = searchService;
        }

        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is WordDetail detail)
            {
                ApplyDetail(detail);
                return;
            }

            if (parameter is string word && !string.IsNullOrWhiteSpace(word))
            {
                var loaded = await _searchService.searchAWord(word);
                if (loaded != null)
                    ApplyDetail(loaded);
            }
        }

        public void OnNavigatedFrom()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
        }

        private void ApplyDetail(WordDetail detail)
        {
            WordDetail = detail;
            WordTitle = CapitalizeFirstLetter(detail.Word ?? string.Empty);
            PhoneticUs = string.IsNullOrWhiteSpace(detail.Phonetic) ? "—" : detail.Phonetic;
            PhoneticUk = string.IsNullOrWhiteSpace(detail.Phonetic) ? "—" : detail.Phonetic;
            UsAudioUrl = detail.AudioUs ?? string.Empty;
            UkAudioUrl = detail.AudioUk ?? string.Empty;
            WordImage = null;
            HasWordImage = false;
            if (!string.IsNullOrWhiteSpace(detail.ImageUrl))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(detail.ImageUrl.Trim(), UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bmp.EndInit();
                    bmp.Freeze();
                    WordImage = bmp;
                    HasWordImage = true;
                }
                catch
                {
                    WordImage = null;
                    HasWordImage = false;
                }
            }
        }

        [RelayCommand]
        private void PlayAudioUs() => PlayAudioCore(UsAudioUrl, "US");

        [RelayCommand]
        private void PlayAudioUk() => PlayAudioCore(UkAudioUrl, "UK");

        [RelayCommand]
        private void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
        }

        [RelayCommand]
        private void SaveWord()
        {
        }

        [RelayCommand]
        private void OpenNote()
        {
        }

        [RelayCommand]
        private void ShareWord()
        {
            if (WordDetail?.Word == null) return;
            Clipboard.SetText(WordDetail.Word);
            MessageBox.Show("Word copied successfully");
        }

        [RelayCommand]
        private async Task SearchRelatedWord(string? word)
        {
            if (string.IsNullOrWhiteSpace(word)) return;
            var detail = await _searchService.searchAWord(word);
            if (detail != null)
                _navigationService.NavigateTo<DetailsPage, DetailsPageViewModel>(detail);
        }

        private void PlayAudioCore(string url, string accent)
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
