using Royal_Blueberry_Dictionary.Model.Settings;
using System;
using System.IO;
using System.Text.Json;

namespace Royal_Blueberry_Dictionary.Service
{
    /// <summary>
    /// Quản lý lưu/load settings từ file JSON
    /// Singleton pattern
    /// </summary>
    public class SettingsService
    {
        #region Singleton

        private static SettingsService _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        #endregion

        #region Fields & Properties

        private readonly string _settingsPath;
        private readonly string _settingsDirectory;

        /// <summary>
        /// Settings hiện tại của app
        /// </summary>
        public AppSettings CurrentSettings { get; private set; }

        #endregion

        #region Constructor

        private SettingsService()
        {
            // Lưu vào %LocalAppData%/RoyalBlueberryDictionary/settings.json
            _settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RoyalBlueberryDictionary"
            );

            _settingsPath = Path.Combine(_settingsDirectory, "settings.json");

            LoadSettings();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load settings từ file (hoặc tạo mới nếu chưa có)
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                    System.Diagnostics.Debug.WriteLine($"✅ Settings loaded from: {_settingsPath}");
                }
                else
                {
                    CurrentSettings = new AppSettings(); // Default settings
                    SaveSettings(); // Tạo file mới
                    System.Diagnostics.Debug.WriteLine($"✅ Created new settings file at: {_settingsPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error loading settings: {ex.Message}");
                CurrentSettings = new AppSettings();
            }
        }

        /// <summary>
        /// Lưu settings vào file JSON
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // Tạo folder nếu chưa có
                Directory.CreateDirectory(_settingsDirectory);

                // Serialize với indent để dễ đọc
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                string json = JsonSerializer.Serialize(CurrentSettings, options);
                File.WriteAllText(_settingsPath, json);

                System.Diagnostics.Debug.WriteLine($"✅ Settings saved to: {_settingsPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset về default settings
        /// </summary>
        public void ResetToDefault()
        {
            CurrentSettings = new AppSettings();
            SaveSettings();
            System.Diagnostics.Debug.WriteLine("✅ Settings reset to default");
        }

        #endregion

        #region Shortcut Methods

        /// <summary>
        /// Lưu theme mode
        /// </summary>
        public void SaveThemeMode(ThemeMode mode)
        {
            CurrentSettings.ThemeMode = mode.ToString();
            SaveSettings();
        }

        /// <summary>
        /// Lưu color theme (preset hoặc custom)
        /// </summary>
        public void SaveColorTheme(string themeName, AppColorTheme customTheme = null)
        {
            CurrentSettings.ColorTheme = themeName;
            CurrentSettings.CustomColorTheme = customTheme;
            SaveSettings();
        }

        /// <summary>
        /// Lưu font family
        /// </summary>
        public void SaveFontFamily(string fontFamily)
        {
            CurrentSettings.FontFamily = fontFamily;
            SaveSettings();
        }

        /// <summary>
        /// Lưu font size
        /// </summary>
        public void SaveFontSize(double fontSize)
        {
            CurrentSettings.FontSize = fontSize;
            SaveSettings();
        }

        /// <summary>
        /// Lưu cả font family + size cùng lúc
        /// </summary>
        public void SaveFont(string fontFamily, double fontSize)
        {
            CurrentSettings.FontFamily = fontFamily;
            CurrentSettings.FontSize = fontSize;
            SaveSettings();
        }

        #endregion
    }
}