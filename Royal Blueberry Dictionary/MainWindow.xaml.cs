using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using Royal_Blueberry_Dictionary.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NavigationService = Royal_Blueberry_Dictionary.Service.NavigationService;

namespace Royal_Blueberry_Dictionary
{
    public partial class MainWindow : Window
    {
        private bool isSidebarOpen;
        private readonly SearchViewModel searchViewModel;
        private readonly NavigationService navigationService;
        private readonly AuthService authService;

        public MainWindow()
        {
            InitializeComponent();

            searchViewModel = App.serviceProvider.GetRequiredService<SearchViewModel>();
            navigationService = App.serviceProvider.GetRequiredService<NavigationService>();
            authService = App.serviceProvider.GetRequiredService<AuthService>();

            navigationService.SetMainFrame(MainFrame);
            DataContext = searchViewModel;

            Closed += MainWindow_Closed;
            authService.AuthStateChanged += OnAuthStateChanged;

            navigationService.NavigateTo<HomePage, SearchViewModel>("home");
            RefreshAuthSummary();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            authService.AuthStateChanged -= OnAuthStateChanged;
        }

        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isSidebarOpen)
            {
                CloseSidebar();
            }
            else
            {
                OpenSidebar();
            }
        }

        private void SideBarNavigate(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            navigationService.NavigateByTag(button.Tag?.ToString());
            CloseSidebar();
        }

        private void OpenSidebar()
        {
            isSidebarOpen = true;
            Overlay.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation
            {
                From = -280,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void CloseSidebar()
        {
            isSidebarOpen = false;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = -280,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            animation.Completed += (_, _) => Overlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseSidebar();
        }

        private void BackBtn_Click_1(object sender, RoutedEventArgs e)
        {
            navigationService.GoBack();
        }

        private void ForwardBtn_Click_1(object sender, RoutedEventArgs e)
        {
            navigationService.GoForward();
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.NavigationService?.Refresh();
        }

        private void OnAuthStateChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(RefreshAuthSummary);
        }

        private void RefreshAuthSummary()
        {
            if (authService.IsAuthenticated)
            {
                SidebarUserStatus.Text = authService.CurrentUser?.Name ?? "Signed in";
                SidebarUserHint.Text = authService.CurrentUser?.Email ?? "Authenticated with JWT";
                return;
            }

            SidebarUserStatus.Text = "Guest mode";
            SidebarUserHint.Text = "Open Account to sign in or create an account";
        }
    }
}
