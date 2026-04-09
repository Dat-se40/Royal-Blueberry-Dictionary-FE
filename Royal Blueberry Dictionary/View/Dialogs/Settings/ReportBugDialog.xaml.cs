using Royal_Blueberry_Dictionary.Model.Settings;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    /// <summary>
    /// Report Bug Dialog - Hướng dẫn user report bug
    /// </summary>
    public partial class ReportBugDialog : Window
    {
        #region Constants

        private const string GITHUB_ISSUES = "https://github.com/Dat-se40/Royal-Blueberry-Dictionary-FE/issues/new";

        #endregion

        #region Constructor

        public ReportBugDialog()
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

        private void OpenGitHubIssues_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GITHUB_ISSUES,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open GitHub:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Private Methods

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