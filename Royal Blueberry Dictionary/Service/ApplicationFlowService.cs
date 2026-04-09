using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Royal_Blueberry_Dictionary.Service
{
    public class ApplicationFlowService
    {
        private readonly IServiceProvider serviceProvider;
        private WelcomeWindow? welcomeWindow;
        private MainWindow? mainWindow;

        public ApplicationFlowService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void ShowWelcomeWindow()
        {
            if (welcomeWindow == null)
            {
                welcomeWindow = serviceProvider.GetRequiredService<WelcomeWindow>();
                welcomeWindow.Closed += OnWelcomeWindowClosed;
            }

            Application.Current.MainWindow = welcomeWindow;

            if (!welcomeWindow.IsVisible)
            {
                welcomeWindow.Show();
            }
            else
            {
                welcomeWindow.Activate();
            }
        }

        public void EnterMainApp()
        {
            if (mainWindow == null)
            {
                mainWindow = serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Closed += OnMainWindowClosed;
            }

            Application.Current.MainWindow = mainWindow;

            if (!mainWindow.IsVisible)
            {
                mainWindow.Show();
            }
            else
            {
                mainWindow.Activate();
            }

            if (welcomeWindow != null)
            {
                var closingWindow = welcomeWindow;
                welcomeWindow = null;
                closingWindow.Closed -= OnWelcomeWindowClosed;
                closingWindow.Close();
            }
        }

        private void OnWelcomeWindowClosed(object? sender, EventArgs e)
        {
            if (sender is WelcomeWindow window)
            {
                window.Closed -= OnWelcomeWindowClosed;
            }

            welcomeWindow = null;
        }

        private void OnMainWindowClosed(object? sender, EventArgs e)
        {
            if (sender is MainWindow window)
            {
                window.Closed -= OnMainWindowClosed;
            }

            mainWindow = null;
        }
    }
}
