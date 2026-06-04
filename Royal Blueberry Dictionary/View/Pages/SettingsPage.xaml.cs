using Royal_Blueberry_Dictionary.ViewModel;
using System.Windows;
using System.Windows.Controls;
using Royal_Blueberry_Dictionary.Service;
using System.Windows.Media;
using System;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// Code-behind tối giản theo MVVM pattern
    /// Chỉ xử lý event cho ComboBox vì WPF ComboBox không support Command tốt
    /// </summary>
    public partial class SettingsPage : Page
    {

        private bool _isInitializing = true;
        private bool _isResetting = false;

        #region Properties

        /// <summary>
        /// ViewModel reference (null-safe)
        /// </summary>
        private SettingsPageViewModel ViewModel => DataContext as SettingsPageViewModel;

        #endregion

        #region Constructor

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }


        #endregion

        #region Event Handlers - Color Theme

        /// <summary>
        /// Xử lý khi user chọn Color Theme ComboBox
        /// </summary>
        private void ColorThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _isResetting) return;
            if (ColorThemeComboBox == null || ViewModel == null) return;

            if (ColorThemeComboBox.SelectedItem is not ComboBoxItem item) return;

            string tag = item.Tag?.ToString() ?? string.Empty;

            switch (tag)
            {
                case "theme_active":
                    System.Diagnostics.Debug.WriteLine("✅ Already using custom theme");
                    return;

                case "preset_picker":
                    ViewModel.OpenThemePresetDialogCommand?.Execute(null);

                    _isResetting = true;
                    LoadCurrentSettings();
                    _isResetting = false;
                    break;

                case "custom_picker":
                    ViewModel.OpenCustomThemeDialogCommand?.Execute(null);

                    _isResetting = true;
                    LoadCurrentSettings();
                    _isResetting = false;
                    break;

                case "default":
                    var settings = SettingsService.Instance.CurrentSettings;

                    bool isDefaultTheme =
                        string.IsNullOrWhiteSpace(settings.ColorTheme) ||
                        settings.ColorTheme == "default";

                    if (!isDefaultTheme)
                    {
                        ViewModel.ResetToDefaultColorsCommand?.Execute(null);
                    }

                    _isResetting = true;
                    LoadCurrentSettings();
                    _isResetting = false;
                    break;
            }
        }


        #endregion

        #region Event Handlers - Font

        /// <summary>
        /// Xử lý khi user chọn Font ComboBox
        /// </summary>
        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _isResetting) return;
            if (FontFamilyComboBox == null || ViewModel == null) return;

            if (FontFamilyComboBox.SelectedItem is not ComboBoxItem item) return;

            string tag = item.Tag?.ToString() ?? string.Empty;

            switch (tag)
            {
                case "custom_active":
                    System.Diagnostics.Debug.WriteLine("✅ Already using custom font");
                    return;

                case "custom_picker":
                    ViewModel.OpenFontPickerDialogCommand?.Execute(null);

                    _isResetting = true;
                    ApplyGlobalFont();
                    LoadCurrentSettings();
                    _isResetting = false;
                    break;

                case "default":
                    var settings = SettingsService.Instance.CurrentSettings;

                    bool isDefaultFont =
                        string.IsNullOrWhiteSpace(settings.FontFamily) ||
                        (settings.FontFamily == "Segoe UI" && Math.Abs(settings.FontSize - 14) < 0.01);

                    if (!isDefaultFont)
                    {
                        ViewModel.ResetToDefaultFontCommand?.Execute(null);
                    }

                    _isResetting = true;
                    ApplyGlobalFont();
                    LoadCurrentSettings();
                    _isResetting = false;
                    break;
            }
        }

        #endregion

        #region LoadFontSettings - Font
        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitializing = true;

            ApplyGlobalFont();
            LoadCurrentSettings();

            _isInitializing = false;
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

        private void LoadCurrentSettings()
        {
            var settings = SettingsService.Instance.CurrentSettings;
            // ================= COLOR THEME =================
            bool isDefaultTheme =
                string.IsNullOrWhiteSpace(settings.ColorTheme) ||
                settings.ColorTheme == "default";

            if (isDefaultTheme)
            {
                HideAllActiveItems(ColorThemeComboBox);
                ColorThemeComboBox.SelectedIndex = 0;
            }
            else
            {
                ShowActiveItem(ColorThemeComboBox, 1);
                ColorThemeComboBox.SelectedIndex = 1;
            }

            // ================= FONT =================

            bool isDefaultFont =
                string.IsNullOrWhiteSpace(settings.FontFamily) ||
                (settings.FontFamily == "Segoe UI" && Math.Abs(settings.FontSize - 14) < 0.01);

            if (isDefaultFont)
            {
                HideAllActiveItems(FontFamilyComboBox);
                FontFamilyComboBox.SelectedIndex = 0;
            }
            else
            {
                ShowActiveItem(FontFamilyComboBox, 1);
                FontFamilyComboBox.SelectedIndex = 1;
            }
        }
        private void ShowActiveItem(ComboBox comboBox, int activeIndex)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ComboBoxItem item)
                {
                    string tag = item.Tag?.ToString() ?? string.Empty;

                    if (tag.EndsWith("_active"))
                    {
                        item.Visibility = i == activeIndex ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        private void HideAllActiveItems(ComboBox comboBox)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ComboBoxItem item)
                {
                    string tag = item.Tag?.ToString() ?? string.Empty;

                    if (tag.EndsWith("_active"))
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        #endregion
    }
}