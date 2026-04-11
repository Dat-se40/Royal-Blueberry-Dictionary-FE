namespace Royal_Blueberry_Dictionary.Model.Settings
{
    /// <summary>
    /// Model lưu tất cả settings của app
    /// </summary>
    public class AppSettings
    {
        #region Theme Settings

        /// <summary>
        /// Theme mode: "Light" | "Dark" | "System"
        /// </summary>
        public string ThemeMode { get; set; } = "Light";

        /// <summary>
        /// Color theme name: "default" | "theme1" | "theme2" | ... | "custom"
        /// </summary>
        public string ColorTheme { get; set; } = "default";

        /// <summary>
        /// Custom color theme (nếu ColorTheme = "custom")
        /// </summary>
        public AppColorTheme CustomColorTheme { get; set; }

        #endregion

        #region Appearance Settings

        /// <summary>
        /// Font family name
        /// </summary>
        public string FontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// Font size
        /// </summary>
        public double FontSize { get; set; } = 14;

        #endregion

        #region Data Settings

        /// <summary>
        /// Tự động lưu lịch sử tra cứu
        /// </summary>
        public bool AutoSaveHistory { get; set; } = true;

        /// <summary>
        /// Giới hạn số từ yêu thích
        /// </summary>
        public int FavouriteLimit { get; set; } = 500;

        #endregion
    }
}