using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Dialogs.Settings;
using System.Windows;
using AppThemeMode = Royal_Blueberry_Dictionary.Service.ThemeMode;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        #region Dependencies

        private readonly ThemeManager _themeManager;
        private readonly SettingsService _settingsService;

        #endregion

        #region Properties

        [ObservableProperty]
        private int _themeModeIndex = 0;

        #endregion

        #region Constructor

        public SettingsPageViewModel(ThemeManager themeManager)
        {
            _themeManager = themeManager;
            _settingsService = SettingsService.Instance;
            LoadCurrentSettings();
        }

        #endregion

        #region Load Settings

        private void LoadCurrentSettings()
        {
            var settings = _settingsService.CurrentSettings;

            // Load Theme Mode
            ThemeModeIndex = settings.ThemeMode switch
            {
                "Light" => 0,
                "Dark" => 1,
                "System" => 2,
                _ => 0
            };

            // Apply theme if needed
            if (!string.IsNullOrEmpty(settings.ColorTheme) && settings.ColorTheme != "default")
            {
                if (settings.ColorTheme == "custom" && settings.CustomColorTheme != null)
                {
                    _themeManager.ApplyCustomColorTheme(
                        settings.CustomColorTheme.Primary,
                        settings.CustomColorTheme.Secondary,
                        settings.CustomColorTheme.Accent
                    );
                }
                else
                {
                    _themeManager.ApplyColorTheme(settings.ColorTheme);
                }
            }

            // Apply font if custom
            if (!string.IsNullOrEmpty(settings.FontFamily) && settings.FontFamily != "Segoe UI")
            {
                var fontFamily = new System.Windows.Media.FontFamily(settings.FontFamily);
                Application.Current.Resources["AppFontFamily"] = fontFamily;
                Application.Current.Resources["AppFontSize"] = settings.FontSize;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Loaded settings: Theme={settings.ThemeMode}, Color={settings.ColorTheme}, Font={settings.FontFamily} {settings.FontSize}pt");
        }

        #endregion

        #region Theme Commands

        /// <summary>
        /// Xử lý khi user thay đổi Theme Mode
        /// </summary>
        partial void OnThemeModeIndexChanged(int value)
        {
            // ✅ SỬ DỤNG ALIAS
            AppThemeMode mode = value switch
            {
                0 => AppThemeMode.Light,
                1 => AppThemeMode.Dark,
                2 => AppThemeMode.System,
                _ => AppThemeMode.Light
            };

            _themeManager.SetThemeMode(mode);
            System.Diagnostics.Debug.WriteLine($"✅ Theme mode changed to: {mode}");
        }

        [RelayCommand]
        private void OpenThemePresetDialog()
        {
            var dialog = new ThemePresetDialog(_themeManager)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show($"Theme '{dialog.SelectedTheme}' applied successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void OpenCustomThemeDialog()
        {
            var dialog = new CustomThemeDialog(_themeManager)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show("Custom colors applied successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void ResetToDefaultColors()
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset to default colors?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _themeManager.ResetToDefaultColors();
                MessageBox.Show("Colors reset to default successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Font Commands

        [RelayCommand]
        private void OpenFontPickerDialog()
        {
            var dialog = new FontPickerDialog
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                _settingsService.SaveFont(dialog.SelectedFont.Source, dialog.SelectedFontSize);

                MessageBox.Show($"Font '{dialog.SelectedFont.Source}' ({dialog.SelectedFontSize}pt) applied!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void ResetToDefaultFont()
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset to default font (Segoe UI 14pt)?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var defaultFont = new System.Windows.Media.FontFamily("Segoe UI");
                Application.Current.Resources["AppFontFamily"] = defaultFont;
                Application.Current.Resources["AppFontSize"] = 14.0;

                foreach (Window window in Application.Current.Windows)
                {
                    window.FontFamily = defaultFont;
                    window.FontSize = 14;
                }

                _settingsService.SaveFont("Segoe UI", 14);

                MessageBox.Show("Font reset to default successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Help & Support Commands (Stubs)

        // TODO: Implement when dialogs are available

        #endregion
    }
}
