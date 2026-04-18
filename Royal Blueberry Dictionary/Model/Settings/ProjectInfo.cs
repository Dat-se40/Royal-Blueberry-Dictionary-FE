namespace Royal_Blueberry_Dictionary.Model.Settings
{
    /// <summary>
    /// Thông tin dự án (constant)
    /// </summary>
    public static class ProjectInfo
    {
        #region Basic Info

        public const string APP_NAME = "Royal Blueberry Dictionary";
        public const string VERSION = "1.0.0";
        public const string BUILD = "2025.01.15";
        public const string COPYRIGHT = "© 2025 Royal Blueberry Dictionary Team";

        #endregion

        #region Course Info

        public const string COURSE_NAME = "Nhập môn công nghệ phần mềm";
        public const string SEMESTER = "Kỳ 2 - Năm học 2025-2026";
        public const string LECTURER = "ThS. Huỳnh Ngọc Tín";

        #endregion

        #region Team Members

        public static readonly string[] TEAM_MEMBERS = new[]
        {
            "Nguyễn Tấn Đạt",
            "Võ Nguyễn Thanh Hương",
            "Ngô Phương Hiền",
            "Nguyễn Quốc An",
            "Võ Văn Hải"
        };

        #endregion

        #region Links

        public const string GITHUB_FRONTEND = "https://github.com/Dat-se40/Royal-Blueberry-Dictionary-FE";
        public const string GITHUB_BACKEND = "https://github.com/Dat-se40/Royal-Blueberry-Dictionary-BE";
        public const string EMAIL = "labotanique117@gmail.com";

        #endregion

        #region License

        public const string LICENSE_TYPE = "MIT License";
        public const string LICENSE_URL = "https://github.com/Dat-se40/Royal-Blueberry-Dictionary-FE/blob/main/LICENSE";

        #endregion
    }
}