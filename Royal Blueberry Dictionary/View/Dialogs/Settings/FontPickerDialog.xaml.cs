using System;
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

        #endregion

        #region Constructor

        public FontPickerDialog()
        {
            InitializeComponent();
            LoadSystemFonts();
            ApplyGlobalFont();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load all system fonts
        /// </summary>
        private void LoadSystemFonts()
        {
            var fonts = Fonts.SystemFontFamilies
                .OrderBy(f => f.Source)
                .ToList();

            FontListBox.ItemsSource = fonts;

            // Select default font (Segoe UI or first)
            // TODO: Load from SettingsService when available
            var defaultFont = fonts.FirstOrDefault(f => f.Source == "Segoe UI") ?? fonts.FirstOrDefault();
            if (defaultFont != null)
            {
                FontListBox.SelectedItem = defaultFont;
                SelectedFont = defaultFont;
            }
        }

        /// <summary>
        /// Apply global font
        /// </summary>
        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update preview text
        /// </summary>
        private void UpdatePreview()
        {
            if (SelectedFont != null && PreviewText != null)
            {
                PreviewText.FontFamily = SelectedFont;
                PreviewText.FontSize = SelectedFontSize;
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

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem item &&
                double.TryParse(item.Tag?.ToString(), out double size))
            {
                SelectedFontSize = size;
                UpdatePreview();
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFont != null)
            {
                // Apply globally
                Application.Current.Resources["AppFontFamily"] = SelectedFont;
                Application.Current.Resources["AppFontSize"] = SelectedFontSize;

                // Apply to all windows
                foreach (Window window in Application.Current.Windows)
                {
                    window.FontFamily = SelectedFont;
                    window.FontSize = SelectedFontSize;
                }

                // TODO: Save to SettingsService when available
                // _settingsService.CurrentSettings.FontFamily = SelectedFont.Source;
                // _settingsService.CurrentSettings.FontSize = SelectedFontSize;
                // _settingsService.SaveSettings();

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a font!", "Notice",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}