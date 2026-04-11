using Royal_Blueberry_Dictionary.Model.Settings;
using System;
using System.Windows;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.Service
{
    #region ThemeMode Enum

    /// <summary>
    /// Chế độ theme của app
    /// </summary>
    public enum ThemeMode
    {
        Light,
        Dark,
        System
    }

    #endregion

    /// <summary>
    /// Quản lý theme mode (Light/Dark) và color theme của toàn app
    /// Singleton pattern
    /// </summary>
    public class ThemeManager
    {
        #region Singleton

        //private static ThemeManager _instance;
        //public static ThemeManager Instance => _instance ??= new ThemeManager();

        #endregion

        #region Fields & Properties

        private ResourceDictionary _appResources;

        public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;
        public string CurrentColorTheme { get; private set; } = "default";

        private AppColorTheme _currentThemeObject;

        public event Action<ThemeMode> ThemeChanged;

        #endregion

        #region Constructor

        public ThemeManager()
        {
            _appResources = Application.Current.Resources;
            // Default theme: theme1
            _currentThemeObject = ThemePresets.GetTheme("theme1");
        }

        #endregion

        #region Public Methods - Theme Mode

        /// <summary>
        /// Đổi theme mode (Light/Dark/System)
        /// </summary>
        public void SetThemeMode(ThemeMode mode)
        {
            CurrentTheme = mode;

            // TODO: Implement System theme detection
            if (mode == ThemeMode.System)
            {
                mode = ThemeMode.Light; // Fallback
            }

            // Apply theme
            if (CurrentColorTheme == "default" || _currentThemeObject == null)
            {
                ReloadDefaultColors(mode);
            }
            else
            {
                ApplyColorTheme(_currentThemeObject);
            }

            SettingsService.Instance.SaveThemeMode(mode);

            ThemeChanged?.Invoke(mode);

            System.Diagnostics.Debug.WriteLine($"✅ Theme mode changed to: {mode} (ColorTheme: {CurrentColorTheme})");
        }

        #endregion

        #region Public Methods - Color Theme

        /// <summary>
        /// Apply preset color theme (theme1, theme2, ...)
        /// </summary>
        public void ApplyColorTheme(string themeName)
        {
            var theme = ThemePresets.GetTheme(themeName);
            if (theme == null) return;

            CurrentColorTheme = themeName;
            _currentThemeObject = theme;
            ApplyColorTheme(theme);

            SettingsService.Instance.SaveColorTheme(themeName, null);
        }

        /// <summary>
        /// Apply custom color theme (user-defined 3 colors)
        /// </summary>
        public void ApplyCustomColorTheme(Color primary, Color secondary, Color accent)
        {
            var theme = new AppColorTheme
            {
                Primary = primary,
                Secondary = secondary,
                Accent = accent
            };

            CurrentColorTheme = "custom";
            _currentThemeObject = theme;
            ApplyColorTheme(theme);

            // ✅ SAVE TO SETTINGS SERVICE
            SettingsService.Instance.SaveColorTheme("custom", theme);
        }

        /// <summary>
        /// Reset về màu mặc định (từ Colors.xaml)
        /// </summary>
        public void ResetToDefaultColors()
        {
            try
            {
                var colorsDict = new ResourceDictionary
                {
                    Source = new Uri("/Resources/Styles/Colors.xaml", UriKind.Relative)
                };

                string prefix = CurrentTheme == ThemeMode.Dark ? "Dark" : "Light";

                var resourcesToReset = GetAllResourceKeys();

                foreach (var key in resourcesToReset)
                {
                    string sourceKey = $"{prefix}{key}";
                    if (colorsDict.Contains(sourceKey))
                    {
                        _appResources[key] = colorsDict[sourceKey];
                    }
                }

                CurrentColorTheme = "default";
                _currentThemeObject = null;

                // ✅ SAVE TO SETTINGS SERVICE
                SettingsService.Instance.SaveColorTheme("default", null);

                System.Diagnostics.Debug.WriteLine($"✅ Successfully reset to default colors ({prefix} mode)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to reset colors: {ex.Message}");
            }
        }
        #endregion

        #region Private Methods - Apply Theme

        /// <summary>
        /// Apply theme resources based on mode (Light/Dark)
        /// </summary>
        private void ApplyTheme(ThemeMode mode)
        {
            string prefix = mode == ThemeMode.Light ? "Light" : "Dark";

            foreach (var key in GetAllResourceKeys())
            {
                UpdateResource(key, $"{prefix}{key}");
            }
        }

        /// <summary>
        /// Apply color theme object (custom or preset)
        /// </summary>
        private void ApplyColorTheme(AppColorTheme theme)
        {
            string prefix = CurrentTheme == ThemeMode.Light ? "Light" : "Dark";

            if (CurrentTheme == ThemeMode.Light)
            {
                ApplyLightModeColors(theme);
            }
            else
            {
                ApplyDarkModeColors(theme);
            }

            ApplyTheme(CurrentTheme);
            System.Diagnostics.Debug.WriteLine($"✅ Applied color theme: {CurrentColorTheme} ({CurrentTheme} mode)");
        }

        #endregion

        #region Private Methods - Light Mode Colors

        private void ApplyLightModeColors(AppColorTheme theme)
        {
            // Background colors
            Color lightest = LightenColor(theme.Secondary, 0.9);
            Color lighter = LightenColor(theme.Secondary, 0.8);
            Color light = LightenColor(theme.Secondary, 0.7);

            // Gradients
            UpdateGradientBrush(_appResources, "LightMainBackground", lightest, lighter, light);
            UpdateGradientBrush(_appResources, "LightNavbarBackground", theme.Accent, theme.Primary);
            UpdateGradientBrush(_appResources, "LightToolbarBackground", lightest, light);
            UpdateGradientBrush(_appResources, "LightSidebarBackground", Color.FromRgb(255, 255, 255), lightest);
            UpdateGradientBrush(_appResources, "LightCardBackground", Color.FromRgb(255, 255, 255), lightest);
            UpdateGradientBrush(_appResources, "LightWordItemBackground", lightest, light);

            Color hoverColor1 = LightenColor(theme.Secondary, 0.6);
            Color hoverColor2 = LightenColor(theme.Secondary, 0.65);
            UpdateGradientBrush(_appResources, "LightWordItemHover", hoverColor1, hoverColor2);
            UpdateGradientBrush(_appResources, "LightSidebarHover", lightest, light);

            // Solid colors
            UpdateSolidBrush(_appResources, "LightTextColor", theme.Accent);
            UpdateSolidBrush(_appResources, "LightBorderColor", theme.Secondary);
            UpdateSolidBrush(_appResources, "LightButtonColor", theme.Primary);
            UpdateSolidBrush(_appResources, "LightWordBorder", theme.Primary);
            UpdateSolidBrush(_appResources, "LightMeaningBorderLeft", theme.Accent);
            UpdateSolidBrush(_appResources, "LightExampleBorder", theme.Secondary);
            UpdateSolidBrush(_appResources, "LightThemeSliderBackground", theme.Secondary);
            UpdateSolidBrush(_appResources, "LightThemeIconColor", theme.Primary);
            UpdateSolidBrush(_appResources, "LightSidebarHoverText", theme.Accent);

            // Suggestions Popup
            UpdateSolidBrush(_appResources, "LightSuggestionsBackground", Color.FromRgb(255, 255, 255));
            UpdateSolidBrush(_appResources, "LightSuggestionsBorder", theme.Secondary);
            UpdateSolidBrush(_appResources, "LightSuggestionsItemBorder", lightest);
            UpdateSolidBrush(_appResources, "LightSuggestionsItemHover", lightest);
            UpdateSolidBrush(_appResources, "LightSuggestionsItemSelected", hoverColor1);

            // Buttons
            UpdateGradientBrush(_appResources, "LightSearchButton", theme.Primary, theme.Secondary);
            UpdateGradientBrush(_appResources, "LightSearchButtonHover", theme.Accent, theme.Primary);
            UpdateGradientBrush(_appResources, "LightToolButtonActive", theme.Primary, theme.Secondary);
        }

        #endregion

        #region Private Methods - Dark Mode Colors

        private void ApplyDarkModeColors(AppColorTheme theme)
        {
            Color darkest = Color.FromRgb(13, 27, 42);
            Color darker = Color.FromRgb(27, 38, 59);
            Color dark = Color.FromRgb(30, 41, 59);

            // Gradients
            UpdateGradientBrush(_appResources, "DarkMainBackground", darkest, darker);
            UpdateGradientBrush(_appResources, "DarkNavbarBackground",
                DarkenColor(theme.Accent, 0.5), DarkenColor(theme.Primary, 0.5));
            UpdateGradientBrush(_appResources, "DarkToolbarBackground", dark, Color.FromRgb(51, 65, 85));
            UpdateGradientBrush(_appResources, "DarkSidebarBackground", dark, Color.FromRgb(15, 23, 42));
            UpdateGradientBrush(_appResources, "DarkCardBackground", dark, Color.FromRgb(51, 65, 85));

            UpdateSolidBrush(_appResources, "DarkWordItemBackground", Color.FromRgb(45, 55, 72));
            UpdateSolidBrush(_appResources, "DarkWordItemHover", Color.FromRgb(55, 65, 81));
            UpdateSolidBrush(_appResources, "DarkSidebarHover", Color.FromRgb(51, 65, 85));

            // Solid colors
            UpdateSolidBrush(_appResources, "DarkTextColor", theme.Secondary);
            UpdateSolidBrush(_appResources, "DarkBorderColor", theme.Primary);
            UpdateSolidBrush(_appResources, "DarkButtonColor", theme.Secondary);
            UpdateSolidBrush(_appResources, "DarkWordBorder", theme.Secondary);
            UpdateSolidBrush(_appResources, "DarkMeaningBorderLeft", theme.Primary);
            UpdateSolidBrush(_appResources, "DarkExampleBorder", theme.Secondary);
            UpdateSolidBrush(_appResources, "DarkThemeSliderBackground", theme.Primary);
            UpdateSolidBrush(_appResources, "DarkThemeIconColor", theme.Secondary);
            UpdateSolidBrush(_appResources, "DarkSidebarHoverText", theme.Secondary);

            // Suggestions Popup
            UpdateSolidBrush(_appResources, "DarkSuggestionsBackground", dark);
            UpdateSolidBrush(_appResources, "DarkSuggestionsBorder", theme.Primary);
            UpdateSolidBrush(_appResources, "DarkSuggestionsItemBorder", Color.FromRgb(51, 65, 85));
            UpdateSolidBrush(_appResources, "DarkSuggestionsItemHover", Color.FromRgb(51, 65, 85));
            UpdateSolidBrush(_appResources, "DarkSuggestionsItemSelected", Color.FromRgb(71, 85, 105));

            // Buttons
            UpdateGradientBrush(_appResources, "DarkSearchButton", theme.Primary, theme.Secondary);
            UpdateGradientBrush(_appResources, "DarkSearchButtonHover",
                DarkenColor(theme.Primary, 0.2), theme.Primary);
            UpdateGradientBrush(_appResources, "DarkToolButtonActive", theme.Primary, theme.Secondary);
        }

        #endregion

        #region Private Methods - Reload Default

        private void ReloadDefaultColors(ThemeMode mode)
        {
            try
            {
                var colorsDict = new ResourceDictionary
                {
                    Source = new Uri("/Resources/Styles/Colors.xaml", UriKind.Relative)
                };

                string prefix = mode == ThemeMode.Light ? "Light" : "Dark";

                foreach (var key in GetAllResourceKeys())
                {
                    string sourceKey = $"{prefix}{key}";
                    if (colorsDict.Contains(sourceKey))
                    {
                        _appResources[key] = colorsDict[sourceKey];
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✅ Reloaded default colors for {mode} mode");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to reload default colors: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods - Color Manipulation

        private Color LightenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R + (255 - color.R) * factor),
                (byte)(color.G + (255 - color.G) * factor),
                (byte)(color.B + (255 - color.B) * factor)
            );
        }

        private Color DarkenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor))
            );
        }

        #endregion

        #region Helper Methods - Resource Update

        private void UpdateResource(string key, string sourceKey)
        {
            if (_appResources.Contains(sourceKey))
            {
                _appResources[key] = _appResources[sourceKey];
            }
        }

        private void UpdateSolidBrush(ResourceDictionary resources, string key, Color color)
        {
            resources[key] = new SolidColorBrush(color);
        }

        private void UpdateGradientBrush(ResourceDictionary resources, string key,
            Color color1, Color color2, Color? color3 = null)
        {
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            gradient.GradientStops.Add(new GradientStop(color1, 0));
            gradient.GradientStops.Add(new GradientStop(color2, color3.HasValue ? 0.5 : 1));

            if (color3.HasValue)
            {
                gradient.GradientStops.Add(new GradientStop(color3.Value, 1));
            }

            resources[key] = gradient;
        }

        #endregion

        #region Helper Methods - Resource Keys

        private string[] GetAllResourceKeys()
        {
            return new[]
            {
                "MainBackground", "NavbarBackground", "ToolbarBackground",
                "SidebarBackground", "CardBackground", "WordItemBackground",
                "WordItemHover", "MeaningBackground", "MeaningBorder",
                "MeaningBorderLeft", "ExampleBackground", "ExampleBorder",
                "RelatedBackground", "RelatedBorder", "TextColor",
                "BorderColor", "ButtonColor", "WordBorder",
                "SearchBackground", "SearchBorder", "SearchIcon",
                "SearchText", "SearchPlaceholder", "SearchButton",
                "SearchButtonHover", "SuggestionsBackground", "SuggestionsBorder",
                "SuggestionsItemBorder", "SuggestionsItemHover", "SuggestionsItemSelected",
                "ToolButtonActive", "NavButtonColor",
                "NavButtonHover", "HamburgerBackground", "HamburgerHover",
                "HamburgerIcon", "ThemeToggleBackground", "ThemeSliderBackground",
                "ThemeIconColor", "ToolbarBorder", "SidebarHover", "SidebarHoverText"
            };
        }

        #endregion
    }
}