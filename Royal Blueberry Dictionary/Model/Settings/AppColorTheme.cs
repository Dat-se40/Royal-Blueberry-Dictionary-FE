using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.Model.Settings
{
    /// <summary>
    /// Đại diện cho 1 bộ màu custom (gồm 3 màu)
    /// </summary>
    public class AppColorTheme
    {
        /// <summary>
        /// Màu chính (dùng cho button, border chính)
        /// </summary>
        public Color Primary { get; set; }

        /// <summary>
        /// Màu phụ (dùng cho gradient, hover)
        /// </summary>
        public Color Secondary { get; set; }

        /// <summary>
        /// Màu nhấn (dùng cho navbar, dark elements)
        /// </summary>
        public Color Accent { get; set; }
    }
}