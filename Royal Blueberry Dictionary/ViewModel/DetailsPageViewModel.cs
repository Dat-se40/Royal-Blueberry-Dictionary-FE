using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Dialogs;
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
        private readonly WordService _wordService;
        private WordEntry? _snapshot;
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
        private bool _isFavorited;

        [ObservableProperty]
        private ImageSource? _wordImage;

        [ObservableProperty]
        private bool _hasWordImage;
        #endregion

        #region Constructor
        public DetailsPageViewModel(NavigationService navigationService, SearchService searchService, WordService wordService)
        {
            _navigationService = navigationService;
            _searchService = searchService;
            _wordService = wordService;
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
                await ApplyDetailAsync(detail);
                return;
            }

            if (parameter is string word && !string.IsNullOrWhiteSpace(word))
            {
                try
                {
                    var loaded = await _searchService.searchAWord(word);
                    if (loaded != null)
                    {
                        await ApplyDetailAsync(loaded);
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
        private async Task ApplyDetailAsync(WordDetail detail)
        {
            WordDetail = detail;
            WordTitle = (detail.Word ?? string.Empty).ToUpperInvariant();
            PhoneticUs = string.IsNullOrWhiteSpace(detail.Phonetic) ? "/n/a/" : detail.Phonetic;
            PhoneticUk = string.IsNullOrWhiteSpace(detail.Phonetic) ? "/n/a/" : detail.Phonetic;
            UsAudioUrl = detail.AudioUs ?? string.Empty;
            UkAudioUrl = detail.AudioUk ?? string.Empty;

            LoadWordImage(detail.ImageUrl);

            try
            {
                _snapshot = await _wordService.GetWordEntryByDetail(detail, 0, 0);
                IsFavorited = _snapshot?.IsFavorited ?? false;
            }
            catch
            {
                IsFavorited = false;
            }
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
        private async Task FavoriteAsync()
        {
            if (WordDetail == null) return;

            _snapshot ??= await _wordService.GetWordEntryByDetail(WordDetail, 0, 0);
            if (_snapshot == null) return;

            await _wordService.FavoriteAsync(_snapshot);
            IsFavorited = _snapshot.IsFavorited;
        }

        [RelayCommand]
        private async Task SaveWord()
        {
            if (WordDetail == null) return;

            try
            {
                var dialog = new MeaningSelectorDialog(WordDetail)
                {
                    Owner = Application.Current?.MainWindow
                };

                if (dialog.ShowDialog() != true || dialog.SelectedEntry == null)
                    return;

                var entry = dialog.SelectedEntry;
                entry.UserId ??= "GUEST";

                await _wordService.SmartUpdate(entry);

                if (dialog.SelectedTag != null)
                {
                    var tagService = App.serviceProvider.GetRequiredService<TagService>();
                    await tagService.LinkWordToTagAsync(entry.UserId, entry.Word, entry.MeaningIndex, dialog.SelectedTag.Id);
                }

                MessageBox.Show($"✅ Saved '{entry.Word}' to My Words", "Completed successfully",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void OpenNote()
        {
            if (WordDetail == null) return;

            try
            {
                var dialog = new NoteWriterDialog(WordDetail, meaningIndex: _snapshot?.MeaningIndex ?? 0, definitionIndex: 0)
                {
                    Owner = Application.Current?.MainWindow
                };
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        private Task ToggleFavorite() => FavoriteAsync();
        #endregion
    }
}
