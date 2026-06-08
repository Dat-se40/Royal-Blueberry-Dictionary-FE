using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;
using Royal_Blueberry_Dictionary.ViewModel;
using System.ComponentModel;
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
        private readonly ThemeManager themeManager;

        public MainWindow()
        {
            InitializeComponent();

            searchViewModel = App.serviceProvider.GetRequiredService<SearchViewModel>();
            navigationService = App.serviceProvider.GetRequiredService<NavigationService>();
            authService = App.serviceProvider.GetRequiredService<AuthService>();
            themeManager = App.serviceProvider.GetRequiredService<ThemeManager>();

            navigationService.SetMainFrame(MainFrame);
            DataContext = searchViewModel;

            Closed += MainWindow_Closed;
            authService.AuthStateChanged += OnAuthStateChanged;
            themeManager.ThemeChanged += OnThemeChanged;

            ApplyThemeToggleVisual(themeManager.CurrentTheme == Service.ThemeMode.Dark);

            navigationService.NavigateTo<HomePage, HomePageViewModel>("home");
            RefreshAuthSummary();

            searchViewModel.PropertyChanged += SearchViewModel_PropertyChanged;
        }

        private void SearchViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchViewModel.SelectedSuggestionIndex))
            {
                ScrollSelectedSuggestionIntoView();
            }
        }

        private void ScrollSelectedSuggestionIntoView()
        {
            var index = searchViewModel.SelectedSuggestionIndex;
            if (index < 0 || index >= SuggestionsList.Items.Count)
            {
                return;
            }

            SuggestionsList.ScrollIntoView(SuggestionsList.Items[index]);
        }

        private void SearchInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!searchViewModel.IsSuggestionsOpen || searchViewModel.Suggestions.Count == 0)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Down:
                    searchViewModel.MoveSuggestionDownCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Up:
                    searchViewModel.MoveSuggestionUpCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            searchViewModel.PropertyChanged -= SearchViewModel_PropertyChanged;
            authService.AuthStateChanged -= OnAuthStateChanged;
            themeManager.ThemeChanged -= OnThemeChanged;
        }

        private void ThemeToggle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var newMode = themeManager.CurrentTheme == Service.ThemeMode.Dark
                ? Service.ThemeMode.Light
                : Service.ThemeMode.Dark;
            themeManager.SetThemeMode(newMode);
            ApplyThemeToggleVisual(newMode == Service.ThemeMode.Dark);
        }

        private void OnThemeChanged(Service.ThemeMode mode)
        {
            Dispatcher.Invoke(() => ApplyThemeToggleVisual(mode == Service.ThemeMode.Dark));
        }

        private void ApplyThemeToggleVisual(bool isDark)
        {
            var targetLeft = isDark ? 39.0 : 3.0;
            var animation = new DoubleAnimation(targetLeft, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ThemeSlider.BeginAnimation(Canvas.LeftProperty, animation);
            ThemeIcon.Text = isDark ? "\uE708" : "\uE706";
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
