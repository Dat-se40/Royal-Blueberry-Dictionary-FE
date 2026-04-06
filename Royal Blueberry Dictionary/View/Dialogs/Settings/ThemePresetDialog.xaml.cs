using Royal_Blueberry_Dictionary.Service;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    public partial class ThemePresetDialog : Window
    {
        #region Properties

        public string SelectedTheme { get; private set; }

        private readonly ThemeManager _themeManager;

        #endregion

        #region Constructor

        public ThemePresetDialog(ThemeManager themeManager)
        {
            InitializeComponent();
            _themeManager = themeManager;
            ApplyGlobalFont();
        }

        #endregion

        #region Private Methods

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

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string themeName)
            {
                SelectedTheme = themeName;

                // Apply immediately
                _themeManager.ApplyColorTheme(themeName);

                DialogResult = true;
                Close();
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