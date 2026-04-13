using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Royal_Blueberry_Dictionary
{
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
            DataContext = App.serviceProvider.GetRequiredService<WelcomeWindowViewModel>();
            Loaded += WelcomeWindow_Loaded;
            Closed += WelcomeWindow_Closed;
        }

        private async void WelcomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is WelcomeWindowViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                SyncPasswordBoxes(viewModel);
                await viewModel.InitializeAsync();
            }
        }

        private void WelcomeWindow_Closed(object? sender, EventArgs e)
        {
            if (DataContext is WelcomeWindowViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                viewModel.Dispose();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not WelcomeWindowViewModel viewModel)
            {
                return;
            }

            if (e.PropertyName == nameof(WelcomeWindowViewModel.LoginPassword) ||
                e.PropertyName == nameof(WelcomeWindowViewModel.RegisterPassword))
            {
                SyncPasswordBoxes(viewModel);
            }
        }

        private void SyncPasswordBoxes(WelcomeWindowViewModel viewModel)
        {
            if (LoginPasswordBox.Password != viewModel.LoginPassword)
            {
                LoginPasswordBox.Password = viewModel.LoginPassword;
            }

            if (RegisterPasswordBox.Password != viewModel.RegisterPassword)
            {
                RegisterPasswordBox.Password = viewModel.RegisterPassword;
            }
        }

        private void LoginPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is WelcomeWindowViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.LoginPassword = passwordBox.Password;
            }
        }

        private void RegisterPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is WelcomeWindowViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.RegisterPassword = passwordBox.Password;
            }
        }

        private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            DragMove();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
    }
}
