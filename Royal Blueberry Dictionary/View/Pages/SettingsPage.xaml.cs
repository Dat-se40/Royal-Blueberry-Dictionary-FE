using Royal_Blueberry_Dictionary.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// Code-behind tối giản theo MVVM pattern
    /// Chỉ xử lý event cho ComboBox vì WPF ComboBox không support Command tốt
    /// </summary>
    public partial class SettingsPage : Page
    {
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
            // DataContext sẽ được set bởi NavigationService khi navigate
        }

        #endregion

        #region Event Handlers - Color Theme

        /// <summary>
        /// Xử lý khi user chọn Color Theme ComboBox
        /// </summary>
        private void ColorThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tránh trigger khi đang init
            if (ColorThemeComboBox == null || ViewModel == null) return;

            if (ColorThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                switch (tag)
                {
                    case "preset_picker":
                        // Mở dialog chọn preset theme
                        ViewModel.OpenThemePresetDialogCommand?.Execute(null);
                        // Reset về Default sau khi dialog đóng
                        ColorThemeComboBox.SelectedIndex = 0;
                        break;

                    case "custom_picker":
                        // Mở dialog custom colors
                        ViewModel.OpenCustomThemeDialogCommand?.Execute(null);
                        ColorThemeComboBox.SelectedIndex = 0;
                        break;

                    case "default":
                        // User chọn "Default" → Confirm reset
                        var result = MessageBox.Show(
                            "Reset to default colors?",
                            "Confirm",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            ViewModel.ResetToDefaultColorsCommand?.Execute(null);
                        }
                        break;
                }
            }
        }

        #endregion

        #region Event Handlers - Font

        /// <summary>
        /// Xử lý khi user chọn Font ComboBox
        /// </summary>
        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tránh trigger khi đang init
            if (FontFamilyComboBox == null || ViewModel == null) return;

            if (FontFamilyComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                switch (tag)
                {
                    case "custom_picker":
                        // Mở dialog chọn font
                        ViewModel.OpenFontPickerDialogCommand?.Execute(null);
                        FontFamilyComboBox.SelectedIndex = 0;
                        break;

                    case "default":
                        // User chọn "Default" → Confirm reset
                        var result = MessageBox.Show(
                            "Reset to default font (Segoe UI 14pt)?",
                            "Confirm",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                        );

                        if (result == MessageBoxResult.Yes)
                        {
                            ViewModel.ResetToDefaultFontCommand?.Execute(null);
                        }
                        break;
                }
            }
        }

        #endregion
    }
}