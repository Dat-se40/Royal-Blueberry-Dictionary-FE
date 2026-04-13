using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    public partial class FontPickerDialog : Window
    {
        #region Properties

        public FontFamily SelectedFont { get; private set; }
        public double SelectedFontSize { get; private set; } = 14;

        private const double MinFontSize = 8;
        private const double MaxFontSize = 72;

        #endregion

        #region Constructor

        public FontPickerDialog()
        {
            InitializeComponent();
            LoadSystemFonts();
            ApplyGlobalFont();

            FontSizeTextBox.Text = SelectedFontSize.ToString(CultureInfo.InvariantCulture);
            UpdatePreview();
        }

        #endregion

        #region Private Methods

        private void LoadSystemFonts()
        {
            var fonts = Fonts.SystemFontFamilies
                .OrderBy(f => f.Source)
                .ToList();

            FontListBox.ItemsSource = fonts;

            var currentAppFont = Application.Current.Resources["AppFontFamily"] as FontFamily;
            var defaultFont = currentAppFont != null
                ? fonts.FirstOrDefault(f => f.Source == currentAppFont.Source)
                : null;

            defaultFont ??= fonts.FirstOrDefault(f => f.Source == "Segoe UI") ?? fonts.FirstOrDefault();

            if (defaultFont != null)
            {
                FontListBox.SelectedItem = defaultFont;
                SelectedFont = defaultFont;
            }

            if (Application.Current.Resources.Contains("AppFontSize"))
            {
                SelectedFontSize = (double)Application.Current.Resources["AppFontSize"];
            }
        }

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
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font error: {ex.Message}");
            }
        }

        private void UpdatePreview()
        {
            if (PreviewText == null) return;

            if (SelectedFont != null)
            {
                PreviewText.FontFamily = SelectedFont;
            }

            PreviewText.FontSize = SelectedFontSize;
        }

        private void ApplyValidatedFontSizeFromText()
        {
            if (FontSizeTextBox == null) return;

            string raw = FontSizeTextBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
                return;

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) ||
                double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
            {
                parsed = Math.Max(MinFontSize, Math.Min(MaxFontSize, parsed));
                SelectedFontSize = parsed;
                UpdatePreview();
            }
        }

        private void SyncFontSizeText()
        {
            if (FontSizeTextBox != null)
            {
                FontSizeTextBox.Text = SelectedFontSize.ToString("0.##", CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region Event Handlers

        private void FontListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontListBox.SelectedItem is FontFamily selectedFont)
            {
                SelectedFont = selectedFont;
                UpdatePreview();
            }
        }

        private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyValidatedFontSizeFromText();
        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            SelectedFontSize = Math.Max(MinFontSize, SelectedFontSize - 1);
            SyncFontSizeText();
            UpdatePreview();
        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            SelectedFontSize = Math.Min(MaxFontSize, SelectedFontSize + 1);
            SyncFontSizeText();
            UpdatePreview();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            ApplyValidatedFontSizeFromText();

            if (SelectedFont == null)
            {
                MessageBox.Show("Please select a font!", "Notice",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Application.Current.Resources["AppFontFamily"] = SelectedFont;
            Application.Current.Resources["AppFontSize"] = SelectedFontSize;

            foreach (Window window in Application.Current.Windows)
            {
                window.FontFamily = SelectedFont;
                window.FontSize = SelectedFontSize;
            }

            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}


// TODO: Save to SettingsService when available
// _settingsService.CurrentSettings.FontFamily = SelectedFont.Source;
// _settingsService.CurrentSettings.FontSize = SelectedFontSize;
// _settingsService.SaveSettings();