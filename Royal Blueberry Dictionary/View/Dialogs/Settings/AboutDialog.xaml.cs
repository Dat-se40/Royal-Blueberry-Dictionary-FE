using Royal_Blueberry_Dictionary.Model.Settings;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    public partial class AboutDialog : Window
    {
        #region Constructor

        public AboutDialog()
        {
            InitializeComponent();
            ApplyGlobalFont();
        }

        #endregion

        #region Event Handlers

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void GitHubLink_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl(ProjectInfo.GITHUB_FRONTEND);
        }

        private void EmailLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{ProjectInfo.EMAIL}?subject=Royal Blueberry Dictionary - Feedback",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open email client:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FrontendRepo_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl(ProjectInfo.GITHUB_FRONTEND);
        }

        private void BackendRepo_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl(ProjectInfo.GITHUB_BACKEND);
        }

        #endregion

        #region Private Methods

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