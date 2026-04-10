using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Pages;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class WelcomePageViewModel : ObservableObject, Service.INavigationAware
    {
        private readonly AuthService authService;
        private readonly NavigationService navigationService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WelcomeTitle))]
        [NotifyPropertyChangedFor(nameof(WelcomeSubtitle))]
        [NotifyPropertyChangedFor(nameof(PrimaryActionText))]
        [NotifyPropertyChangedFor(nameof(SignInText))]
        [NotifyPropertyChangedFor(nameof(SignUpText))]
        private bool hasActiveSession;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PrimaryActionText))]
        private bool isRestoringSession;

        [ObservableProperty]
        private string currentDisplayName = "Guest";

        [ObservableProperty]
        private string currentEmail = "Browse without signing in";

        public WelcomePageViewModel(AuthService authService, NavigationService navigationService)
        {
            this.authService = authService;
            this.navigationService = navigationService;
            authService.AuthStateChanged += OnAuthStateChanged;
            SyncState();
        }

        public string WelcomeTitle => HasActiveSession ? "Welcome back" : "Choose how to enter the app";

        public string WelcomeSubtitle => HasActiveSession
            ? "Continue with your saved account, switch to another account, or keep using guest mode."
            : "Sign in, create an account, or continue as a guest for local-only usage.";

        public string PrimaryActionText => IsRestoringSession
            ? "Checking session..."
            : HasActiveSession
                ? $"Continue as {CurrentDisplayName}"
                : "Sign in";

        public string SignInText => HasActiveSession ? "Use another account" : "Sign in";

        public string SignUpText => HasActiveSession ? "Create another account" : "Create account";

        public async void OnNavigatedTo(object parameter)
        {
            await RestoreSessionIfNeededAsync();
        }

        public void OnNavigatedFrom()
        {
        }

        [RelayCommand]
        private void ContinueWithPrimaryAction()
        {
            if (IsRestoringSession)
            {
                return;
            }

            if (HasActiveSession)
            {
                navigationService.NavigateTo<HomePage, SearchViewModel>("home");
                return;
            }

            navigationService.NavigateTo<AccountPage, AccountPageViewModel>("login");
        }

        [RelayCommand]
        private void OpenLogin()
        {
            if (HasActiveSession)
            {
                authService.Logout();
                SyncState();
            }

            navigationService.NavigateTo<AccountPage, AccountPageViewModel>("login");
        }

        [RelayCommand]
        private void OpenSignUp()
        {
            if (HasActiveSession)
            {
                authService.Logout();
                SyncState();
            }

            navigationService.NavigateTo<AccountPage, AccountPageViewModel>("register");
        }

        [RelayCommand]
        private void ContinueAsGuest()
        {
            authService.ContinueAsGuest();
            SyncState();
            navigationService.NavigateTo<HomePage, SearchViewModel>("home");
        }

        private async Task RestoreSessionIfNeededAsync()
        {
            if (IsRestoringSession)
            {
                return;
            }

            IsRestoringSession = true;
            try
            {
                if (TokenManager.HasStoredTokens())
                {
                    await authService.TryRestoreSessionAsync();
                }

                SyncState();
            }
            finally
            {
                IsRestoringSession = false;
            }
        }

        private void OnAuthStateChanged(object? sender, EventArgs e)
        {
            SyncState();
        }

        private void SyncState()
        {
            HasActiveSession = authService.IsAuthenticated;
            CurrentDisplayName = authService.CurrentUser?.Name ?? "Guest";
            CurrentEmail = authService.CurrentUser?.Email ?? "Browse without signing in";
        }
    }
}
