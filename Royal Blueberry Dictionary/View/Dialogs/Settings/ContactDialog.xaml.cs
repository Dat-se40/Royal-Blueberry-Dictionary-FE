using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    /// <summary>
    /// Contact Dialog - Hiển thị thông tin liên hệ và hỗ trợ
    /// </summary>
    public partial class ContactDialog : Window
    {
        #region Constants

        private const string EMAIL = "labotanique117@gmail.com";
        private const string GITHUB_REPO = "https://github.com/Dat-se40/BlueBerry-Dictionary";

        #endregion

        #region Constructor

        public ContactDialog()
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
        /// Mở GitHub repository
        /// </summary>
        private void GitHubLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl(GITHUB_REPO);
        }

        /// <summary>
        /// Mở email client
        /// </summary>
        private void EmailLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{EMAIL}?subject=Royal Blueberry Dictionary - Support",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Cannot open email client:\n{ex.Message}\n\nPlease send an email manually to:\n{EMAIL}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
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