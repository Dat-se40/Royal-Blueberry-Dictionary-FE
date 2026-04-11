using Royal_Blueberry_Dictionary.Service;
using System;
using System.Windows;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    public partial class CustomThemeDialog : Window
    {
        #region Properties

        public Color PrimaryColor { get; private set; }
        public Color SecondaryColor { get; private set; }
        public Color AccentColor { get; private set; }

        private readonly ThemeManager _themeManager;

        #endregion

        #region Constructor

        public CustomThemeDialog(ThemeManager themeManager)
        {
            InitializeComponent();
            _themeManager = themeManager;
            ApplyGlobalFont();
            LoadCurrentCustomColors();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load màu custom hiện tại (nếu có)
        /// </summary>
        private void LoadCurrentCustomColors()
        {
            // TODO: Load from SettingsService when available
            // if (_settingsService.CurrentSettings.CustomColorTheme != null)
            // {
            //     var theme = _settingsService.CurrentSettings.CustomColorTheme;
            //     PrimaryColorPicker.SelectedColor = theme.Primary;
            //     SecondaryColorPicker.SelectedColor = theme.Secondary;
            //     AccentColorPicker.SelectedColor = theme.Accent;
            // }
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

        #endregion

        #region Event Handlers

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            // Get colors with fallback
            PrimaryColor = PrimaryColorPicker.SelectedColor ?? Color.FromRgb(102, 126, 234);
            SecondaryColor = SecondaryColorPicker.SelectedColor ?? Color.FromRgb(118, 75, 162);
            AccentColor = AccentColorPicker.SelectedColor ?? Color.FromRgb(240, 147, 251);

            // Apply immediately
            _themeManager.ApplyCustomColorTheme(PrimaryColor, SecondaryColor, AccentColor);

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