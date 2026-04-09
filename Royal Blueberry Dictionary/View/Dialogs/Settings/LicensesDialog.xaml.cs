using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    /// <summary>
    /// Licenses Dialog - Hiển thị thông tin license và acknowledgments
    /// </summary>
    public partial class LicensesDialog : Window
    {
        #region Constructor

        public LicensesDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Đóng dialog
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Cho phép kéo window bằng header
        /// </summary>
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Mở license của app trên GitHub
        /// </summary>
        private void AppLicense_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://github.com/Dat-se40/Royal-Blueberry-Dictionary-FE/blob/main/LICENSE");
        }

        /// <summary>
        /// Mở Frontend GitHub repository
        /// </summary>
        private void FrontendRepo_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://github.com/Dat-se40/Royal-Blueberry-Dictionary-FE");
        }

        /// <summary>
        /// Mở Backend GitHub repository
        /// </summary>
        private void BackendRepo_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://github.com/Dat-se40/Royal-Blueberry-Dictionary-BE");
        }

        /// <summary>
        /// Mở .NET runtime GitHub
        /// </summary>
        private void DotNetLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://github.com/dotnet/runtime");
        }

        /// <summary>
        /// Mở Newtonsoft.Json
        /// </summary>
        private void NewtonsoftLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://www.newtonsoft.com/json");
        }

        /// <summary>
        /// Mở Google Drive API docs
        /// </summary>
        private void GoogleAPILink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://developers.google.com/drive");
        }

        /// <summary>
        /// Mở Free Dictionary API
        /// </summary>
        private void FreeDictLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://dictionaryapi.dev");
        }

        /// <summary>
        /// Mở Merriam-Webster API
        /// </summary>
        private void MerriamLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://dictionaryapi.com");
        }

        /// <summary>
        /// Mở Cambridge Dictionary
        /// </summary>
        private void CambridgeLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://dictionary.cambridge.org");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Mở URL trong browser mặc định
        /// </summary>
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open link:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Apply font từ App.Current.Resources
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

                System.Diagnostics.Debug.WriteLine($"✅ Applied font to {this.GetType().Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font to dialog error: {ex.Message}");
            }
        }

        #endregion
    }
}