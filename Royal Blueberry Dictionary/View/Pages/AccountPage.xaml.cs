using Royal_Blueberry_Dictionary.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class AccountPage : Page
    {
        public AccountPage()
        {
            InitializeComponent();
            Loaded += AccountPage_Loaded;
            Unloaded += AccountPage_Unloaded;
            DataContextChanged += AccountPage_DataContextChanged;
        }

        private void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            AttachViewModel(DataContext as AccountPageViewModel);
        }

        private void AccountPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachViewModel(DataContext as AccountPageViewModel);
        }

        private void AccountPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DetachViewModel(e.OldValue as AccountPageViewModel);
            AttachViewModel(e.NewValue as AccountPageViewModel);
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not AccountPageViewModel viewModel)
            {
                return;
            }

            if (e.PropertyName == nameof(AccountPageViewModel.LoginPassword) ||
                e.PropertyName == nameof(AccountPageViewModel.RegisterPassword))
            {
                SyncPasswordBoxes(viewModel);
            }
        }

        private void SyncPasswordBoxes(AccountPageViewModel viewModel)
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

        private void LoginPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AccountPageViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.LoginPassword = passwordBox.Password;
            }
        }

        private void RegisterPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AccountPageViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.RegisterPassword = passwordBox.Password;
            }
        }

        private void AttachViewModel(AccountPageViewModel? viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            SyncPasswordBoxes(viewModel);
        }

        private void DetachViewModel(AccountPageViewModel? viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }
}
