using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using INavigationAware = Royal_Blueberry_Dictionary.Service.INavigationAware;

namespace BlueBerryDictionary.ViewModels
{
    public partial class DetailsPageViewModel : ObservableObject, INavigationAware
    {
        #region Fields
        private readonly NavigationService _navigationService;
        private readonly SearchService _searchService;
        private readonly MediaPlayer _mediaPlayer = new();
        private WordEntry snapShot = new(); 
        #endregion

        #region Observable Properties
        [ObservableProperty]
        private WordDetail? _wordDetail;

        [ObservableProperty]
        private string _wordTitle = string.Empty;

        [ObservableProperty]
        private string _phoneticUs = string.Empty;

        [ObservableProperty]
        private string _phoneticUk = string.Empty;

        [ObservableProperty]
        private string _usAudioUrl = string.Empty;

        [ObservableProperty]
        private string _ukAudioUrl = string.Empty;

        [ObservableProperty]
        private bool _isFavorite;

        [ObservableProperty]
        private ImageSource? _wordImage;

        [ObservableProperty]
        private bool _hasWordImage;

        public bool IsFavorited => snapShot.IsFavorited;
        #endregion

        #region Constructor
        public DetailsPageViewModel(NavigationService navigationService, SearchService searchService)
        {
            _navigationService = navigationService;
            _searchService = searchService;
        }
        #endregion

        #region Navigation Lifecycle
        /// <summary>
        /// Gọi khi navigate tới page - có thể nhận WordDetail hoặc word string
        /// </summary>
        public async void OnNavigatedTo(object? parameter)
        {
            if (parameter is WordDetail detail)
            {
                ApplyDetail(detail);
                return;
            }

            if (parameter is string word && !string.IsNullOrWhiteSpace(word))
            {
                try
                {
                    var loaded = await _searchService.searchAWord(word);
                    if (loaded != null)
                    {
                        ApplyDetail(loaded);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading word details: {ex.Message}", "Error");
                }
            }
        }

        /// <summary>
        /// Gọi khi navigate ra khỏi page - cleanup resources
        /// </summary>
        public void OnNavigatedFrom()
        {
            try
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Close();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        #endregion

        #region Business Logic
        /// <summary>
        /// Apply WordDetail data lên view properties
        /// </summary>
        private void ApplyDetail(WordDetail detail)
        {
                WordDetail = detail;
                WordTitle = detail.Word.ToUpper(); // Hoặc CapitalizeFirstLetter
                PhoneticUs = detail.Phonetic ?? "/n/a/";
                PhoneticUk = detail.Phonetic ?? "/n/a/";
                UsAudioUrl = detail.AudioUs;
                UkAudioUrl = detail.AudioUk;
                HasWordImage = !string.IsNullOrEmpty(detail.ImageUrl);

                // Load ảnh async nếu cần
                //_ = LoadImageAsync(detail.ImageUrl);
        }

        /// <summary>
        /// Load word image từ URL
        /// </summary>
        private void LoadWordImage(string? imageUrl)
        {
            WordImage = null;
            HasWordImage = false;

            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(imageUrl.Trim(), UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bmp.EndInit();
                bmp.Freeze();

                WordImage = bmp;
                HasWordImage = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image: {ex.Message}");
                WordImage = null;
                HasWordImage = false;
            }
        }

        /// <summary>
        /// Phát audio (US/UK)
        /// </summary>
        private void PlayAudioCore(string url, string accent)
        {
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show($"No audio available ({accent})", "Notification");
                return;
            }

            try
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Open(new Uri(url, UriKind.Absolute));
                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing audio: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Capitalize first letter của string
        /// </summary>
        private static string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return char.ToUpper(text[0]) + text[1..];
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void PlayAudioUs()
        {
            PlayAudioCore(UsAudioUrl, "US");
        }

        [RelayCommand]
        private void PlayAudioUk()
        {
            PlayAudioCore(UkAudioUrl, "UK");
        }

        [RelayCommand]
        private void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
            // TODO: Persist to database
        }

        [RelayCommand]
        private void SaveWord()
        {
            if (WordDetail?.Word == null)
                return;

            // TODO: Implement save logic
            MessageBox.Show("Word saved to favorites", "Success");
        }

        [RelayCommand]
        private void OpenNote()
        {
            // TODO: Open note editor dialog
        }

        [RelayCommand]
        private void ShareWord()
        {
            if (WordDetail?.Word == null)
                return;

            try
            {
                Clipboard.SetText(WordDetail.Word);
                MessageBox.Show("Word copied to clipboard", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private async Task SearchRelatedWord(string? word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            try
            {
                var detail = await _searchService.searchAWord(word);
                if (detail != null)
                {
                    _navigationService.NavigateTo<DetailsPage, DetailsPageViewModel>(detail);
                }
                else
                {
                    MessageBox.Show($"No definition found for '{word}'", "Not Found");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching word: {ex.Message}", "Error");
            }
        }
        [RelayCommand]
        private async Task FavoriteAsync() 
        {
            var wordService = App.serviceProvider.GetRequiredService<WordService>();
            snapShot = await wordService.GetWordEntryByDetail(WordDetail, 0, 0);
            snapShot.IsFavorited =!snapShot.IsFavorited;
            await wordService.FavoriteAsync(snapShot); 
        }
        #endregion
    }
}
