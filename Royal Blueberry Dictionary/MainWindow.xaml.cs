using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Model;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using Royal_Blueberry_Dictionary.ViewModel;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NavigationService = Royal_Blueberry_Dictionary.Service.NavigationService;

namespace Royal_Blueberry_Dictionary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isSidebarOpen = false;
        private SearchViewModel searchViewModel;
        private NavigationService navigationService;
        private HomePage homePage;
        public bool CanGoBack  => MainFrame.CanGoBack;
        public bool CanGoForward => MainFrame.CanGoForward;     

        public MainWindow()
        {
            InitializeComponent();
            searchViewModel = App.serviceProvider.GetRequiredService<SearchViewModel>();
            navigationService = App.serviceProvider.GetRequiredService<NavigationService>();
            navigationService.SetMainFrame(MainFrame);
            this.DataContext = searchViewModel;
            navigationService.NavigateTo<HomePage, SearchViewModel>("hello");
; 
        }
        /// <summary>
        /// Toggle Sidebar (Hamburger button)
        /// </summary>
        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isSidebarOpen)
                CloseSidebar();
            else
                OpenSidebar();
        }
        private void SideBarNavigate(object sender, RoutedEventArgs e) 
        {
            if (sender is not Button button) return;
            navigationService.NavigateByTag(button.Tag?.ToString());

        }
        /// <summary>
        /// Open Sidebar with animation
        /// </summary>
        private void OpenSidebar()
        {
            _isSidebarOpen = true;
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

        /// <summary>
        /// Close Sidebar with animation
        /// </summary>
        private void CloseSidebar()
        {
            _isSidebarOpen = false;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = -280,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            animation.Completed += (s, e) => Overlay.Visibility = Visibility.Collapsed;
            Sidebar.RenderTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        /// <summary>
        /// Close sidebar when clicking overlay
        /// </summary>
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

        private async void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {

            var auth = App.serviceProvider.GetRequiredService<AuthService>();
           await auth.Login();
        }
    }
}