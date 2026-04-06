using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class NoteWriterDialog : Window
    {
        private readonly WordService _wordService;
        private WordEntry _entry;

        public NoteWriterDialog(WordEntry entry)
        {
            InitializeComponent();

            _wordService = App.serviceProvider.GetRequiredService<WordService>();
            _entry = entry;

            Display();
            ApplyGlobalFont();
        }

        public NoteWriterDialog(WordDetail detail, int meaningIndex = 0, int definitionIndex = 0, string? userId = null)
        {
            InitializeComponent();

            _wordService = App.serviceProvider.GetRequiredService<WordService>();
            _entry = LoadOrCreateEntry(detail, meaningIndex, definitionIndex).GetAwaiter().GetResult();
            _entry.UserId ??= string.IsNullOrWhiteSpace(userId) ? "GUEST" : userId;

            Display();
            ApplyGlobalFont();
        }

        private async Task<WordEntry> LoadOrCreateEntry(WordDetail detail, int meaningIndex, int definitionIndex)
        {
            var entry = await _wordService.GetWordEntryByDetail(detail, meaningIndex, definitionIndex);
            entry ??= WordService.MapWordDetailToWordEntry(detail, meaningIndex, definitionIndex);
            return entry;
        }

        private void Display()
        {
            tbNote.Text = _entry?.Note ?? string.Empty;
            WordTitleText.Text = _entry?.Word?.ToUpperInvariant() ?? "WORD";
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_entry == null)
            {
                DialogResult = false;
                Close();
                return;
            }

            _entry.Note = tbNote.Text ?? string.Empty;
            _entry.LastModifiedAt = DateTime.UtcNow;
            _entry.IsDirty = true;

            await _wordService.SmartUpdate(_entry);

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Thêm font chữ
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    FontFamily = (FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    FontSize = (double)Application.Current.Resources["AppFontSize"];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }
    }
}
