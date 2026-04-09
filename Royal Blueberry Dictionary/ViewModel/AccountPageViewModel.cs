using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Config;
using Royal_Blueberry_Dictionary.Model.Auth;
using Royal_Blueberry_Dictionary.Service;
using Royal_Blueberry_Dictionary.View.Dialogs;
using Royal_Blueberry_Dictionary.View.Pages;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class AccountPageViewModel : ObservableObject, Service.INavigationAware
    {
        private static readonly HttpClient AvatarHttpClient = new();

        private static readonly Brush SuccessBackgroundBrush = new SolidColorBrush(Color.FromRgb(236, 253, 245));
        private static readonly Brush SuccessBorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
        private static readonly Brush SuccessForegroundBrush = new SolidColorBrush(Color.FromRgb(4, 120, 87));
        private static readonly Brush ErrorBackgroundBrush = new SolidColorBrush(Color.FromRgb(254, 242, 242));
        private static readonly Brush ErrorBorderBrush = new SolidColorBrush(Color.FromRgb(248, 113, 113));
        private static readonly Brush ErrorForegroundBrush = new SolidColorBrush(Color.FromRgb(185, 28, 28));

        private readonly AuthService authService;
        private readonly NavigationService navigationService;
        private CancellationTokenSource? statusDismissCts;
        private int avatarLoadVersion;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRegisterMode))]
        [NotifyPropertyChangedFor(nameof(PrimaryActionText))]
        private bool isLoginMode = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PrimaryActionText))]
        private bool isSubmitting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GoogleButtonText))]
        private bool isGoogleBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RefreshSessionText))]
        private bool isRefreshingSession;

        [ObservableProperty]
        private string loginEmail = string.Empty;

        [ObservableProperty]
        private string loginPassword = string.Empty;

        [ObservableProperty]
        private string registerEmail = string.Empty;

        [ObservableProperty]
        private string registerPassword = string.Empty;

        [ObservableProperty]
        private string registerDisplayName = string.Empty;

        [ObservableProperty]
        private string loginEmailError = string.Empty;

        [ObservableProperty]
        private string loginPasswordError = string.Empty;

        [ObservableProperty]
        private string registerEmailError = string.Empty;

        [ObservableProperty]
        private string registerPasswordError = string.Empty;

        [ObservableProperty]
        private string registerDisplayNameError = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
        [NotifyPropertyChangedFor(nameof(StatusBackground))]
        [NotifyPropertyChangedFor(nameof(StatusBorderBrush))]
        [NotifyPropertyChangedFor(nameof(StatusForeground))]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusBackground))]
        [NotifyPropertyChangedFor(nameof(StatusBorderBrush))]
        [NotifyPropertyChangedFor(nameof(StatusForeground))]
        private bool isStatusError = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageSubtitle))]
        [NotifyPropertyChangedFor(nameof(SidebarTitle))]
        [NotifyPropertyChangedFor(nameof(SidebarDescription))]
        [NotifyPropertyChangedFor(nameof(SidebarInitial))]
        [NotifyPropertyChangedFor(nameof(SidebarSessionSummary))]
        [NotifyPropertyChangedFor(nameof(SignedInSummary))]
        private bool isAuthenticated;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasAvatarImage))]
        private BitmapImage? avatarImage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AvatarInitial))]
        [NotifyPropertyChangedFor(nameof(SidebarInitial))]
        private string currentDisplayName = "Guest";

        [ObservableProperty]
        private string currentEmail = "Browse without signing in";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RoleDisplay))]
        private string currentRole = "GUEST";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShortUserId))]
        private string currentUserId = "GUEST";

        [ObservableProperty]
        private string currentAvatarUrl = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SignedInSummary))]
        private string sessionState = "Guest";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SidebarSessionSummary))]
        private string tokenSummary = "No active session on this device.";

        [ObservableProperty]
        private string apiBaseUrl = string.Empty;

        public AccountPageViewModel(AuthService authService, NavigationService navigationService, ApiSettings apiSettings)
        {
            this.authService = authService;
            this.navigationService = navigationService;
            ApiBaseUrl = apiSettings.BaseUrl;
            authService.AuthStateChanged += OnAuthStateChanged;
            SyncFromAuthService();
        }

        public bool IsRegisterMode => !IsLoginMode;

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public bool HasAvatarImage => AvatarImage != null;

        public string PrimaryActionText => IsSubmitting
            ? (IsLoginMode ? "Signing in..." : "Creating account...")
            : (IsLoginMode ? "Sign in" : "Create account");

        public string GoogleButtonText => IsGoogleBusy ? "Connecting to Google..." : "Continue with Google";

        public string RefreshSessionText => IsRefreshingSession ? "Refreshing..." : "Refresh session";

        public string PageSubtitle => IsAuthenticated
            ? "Manage your profile and the session saved on this device."
            : "Sign in to sync your learning progress, saved words, and account data.";

        public string SidebarTitle => IsAuthenticated ? "Your account" : "Blueberry Account";

        public string SidebarDescription => IsAuthenticated
            ? "This device is already connected to your Blueberry account. Review the saved session details below or sign out when you need to switch accounts."
            : "Use your email and password or continue with Google. Session handling runs automatically in the background.";

        public string SidebarInitial => IsAuthenticated ? AvatarInitial : "BB";

        public string SidebarSessionSummary => IsAuthenticated
            ? "Saved on this device"
            : "No active session saved";

        public string AvatarInitial
        {
            get
            {
                var seed = IsAuthenticated ? CurrentDisplayName : "Guest";
                if (string.IsNullOrWhiteSpace(seed))
                {
                    return "G";
                }

                return seed.Trim()[0].ToString().ToUpperInvariant();
            }
        }

        public string SignedInSummary => IsAuthenticated
            ? "Signed in on this device"
            : SessionState;

        public string RoleDisplay => FormatRole(CurrentRole);

        public string ShortUserId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentUserId) || string.Equals(CurrentUserId, "GUEST", StringComparison.OrdinalIgnoreCase))
                {
                    return "Not available";
                }

                return CurrentUserId.Length <= 18
                    ? CurrentUserId
                    : $"{CurrentUserId[..10]}...{CurrentUserId[^6..]}";
            }
        }

        public Brush StatusBackground => IsStatusError ? ErrorBackgroundBrush : SuccessBackgroundBrush;

        public Brush StatusBorderBrush => IsStatusError ? ErrorBorderBrush : SuccessBorderBrush;

        public Brush StatusForeground => IsStatusError ? ErrorForegroundBrush : SuccessForegroundBrush;

        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is string mode)
            {
                IsLoginMode = !string.Equals(mode, "register", StringComparison.OrdinalIgnoreCase) &&
                              !string.Equals(mode, "signup", StringComparison.OrdinalIgnoreCase);
            }

            ClearErrors();
            ClearStatus();
            LoginPassword = string.Empty;
            RegisterPassword = string.Empty;
            await EnsureSessionLoadedAsync();
        }

        public void OnNavigatedFrom()
        {
            ClearStatus();
        }

        [RelayCommand]
        private void SwitchToLogin()
        {
            IsLoginMode = true;
            ClearErrors();
            ClearStatus();
        }

        [RelayCommand]
        private void SwitchToRegister()
        {
            IsLoginMode = false;
            ClearErrors();
            ClearStatus();
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (IsSubmitting || IsGoogleBusy || IsRefreshingSession)
            {
                return;
            }

            ClearErrors();
            ClearStatus();

            if (IsLoginMode)
            {
                if (!ValidateLoginInputs())
                {
                    return;
                }

                IsSubmitting = true;
                try
                {
                    var result = await authService.LoginAsync(new LoginRequest
                    {
                        Email = LoginEmail.Trim(),
                        Password = LoginPassword
                    });

                    await ApplyAuthResultAsync(result, "Signed in successfully.");
                }
                finally
                {
                    IsSubmitting = false;
                }

                return;
            }

            if (!ValidateRegisterInputs())
            {
                return;
            }

            IsSubmitting = true;
            try
            {
                var result = await authService.RegisterAsync(new RegisterRequest
                {
                    Email = RegisterEmail.Trim(),
                    Password = RegisterPassword,
                    DisplayName = string.IsNullOrWhiteSpace(RegisterDisplayName) ? null : RegisterDisplayName.Trim()
                });

                await ApplyAuthResultAsync(result, "Account created successfully.");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        [RelayCommand]
        private async Task SignInWithGoogleAsync()
        {
            if (IsSubmitting || IsGoogleBusy || IsRefreshingSession)
            {
                return;
            }

            ClearErrors();
            ClearStatus();

            IsGoogleBusy = true;
            try
            {
                var result = await authService.LoginWithGoogleAsync();
                await ApplyAuthResultAsync(result, "Google sign-in completed.");
            }
            finally
            {
                IsGoogleBusy = false;
            }
        }

        [RelayCommand]
        private void ContinueAsGuest()
        {
            ClearFormInputs();
            authService.ContinueAsGuest();
            SyncFromAuthService();
            navigationService.NavigateTo<HomePage, SearchViewModel>("home");
        }

        [RelayCommand]
        private async Task ReloadProfileAsync()
        {
            ClearStatus();
            var result = await authService.LoadCurrentUserAsync();
            await ApplyAuthResultAsync(result, "Profile refreshed.");
        }

        [RelayCommand]
        private async Task RefreshSessionAsync()
        {
            if (IsRefreshingSession)
            {
                return;
            }

            ClearStatus();
            IsRefreshingSession = true;
            try
            {
                var result = await authService.RefreshSessionAsync();
                await ApplyAuthResultAsync(result, "Session refreshed.");
            }
            finally
            {
                IsRefreshingSession = false;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            if (!ConfirmationDialog.Show(
                    titleText: "Log out",
                    headingText: "Leave this account?",
                    messageText: "Log out and clear the saved tokens on this device. You can sign in again at any time.",
                    primaryButtonText: "Log out",
                    secondaryButtonText: "Keep session",
                    isDanger: true))
            {
                return;
            }

            ClearFormInputs();
            authService.Logout();
            SyncFromAuthService();
            SetStatus("Logged out successfully.", false);
            navigationService.NavigateTo<HomePage, SearchViewModel>("home");
        }

        private async Task EnsureSessionLoadedAsync()
        {
            if (authService.IsAuthenticated)
            {
                SyncFromAuthService();
                await LoadAvatarAsync(CurrentAvatarUrl);
                return;
            }

            if (!TokenManager.HasStoredTokens())
            {
                SyncFromAuthService();
                return;
            }

            var restored = await authService.TryRestoreSessionAsync();
            SyncFromAuthService();

            if (restored)
            {
                await LoadAvatarAsync(CurrentAvatarUrl);
            }
        }

        private async Task ApplyAuthResultAsync(AuthResult result, string successMessage)
        {
            if (!result.IsSuccess)
            {
                ApplyValidationErrors(result.ValidationErrors);
                SetStatus(result.ErrorMessage ?? "Authentication failed.", true);
                SyncFromAuthService();
                return;
            }

            ClearFormInputs();
            SyncFromAuthService();
            await LoadAvatarAsync(CurrentAvatarUrl);
            SetStatus(successMessage, false);
        }

        private void ApplyValidationErrors(Dictionary<string, string> validationErrors)
        {
            foreach (var pair in validationErrors)
            {
                switch (pair.Key.Trim().ToLowerInvariant())
                {
                    case "email":
                        if (IsLoginMode)
                        {
                            LoginEmailError = pair.Value;
                        }
                        else
                        {
                            RegisterEmailError = pair.Value;
                        }
                        break;
                    case "password":
                        if (IsLoginMode)
                        {
                            LoginPasswordError = pair.Value;
                        }
                        else
                        {
                            RegisterPasswordError = pair.Value;
                        }
                        break;
                    case "displayname":
                    case "display_name":
                    case "display-name":
                    case "name":
                        RegisterDisplayNameError = pair.Value;
                        break;
                }
            }
        }

        private bool ValidateLoginInputs()
        {
            var isValid = true;

            if (!IsValidEmail(LoginEmail))
            {
                LoginEmailError = "Enter a valid email address.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(LoginPassword))
            {
                LoginPasswordError = "Password is required.";
                isValid = false;
            }

            return isValid;
        }

        private bool ValidateRegisterInputs()
        {
            var isValid = true;

            if (!IsValidEmail(RegisterEmail))
            {
                RegisterEmailError = "Enter a valid email address.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(RegisterPassword) || RegisterPassword.Length < 6 || RegisterPassword.Length > 100)
            {
                RegisterPasswordError = "Password must be between 6 and 100 characters.";
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(RegisterDisplayName) && RegisterDisplayName.Trim().Length > 80)
            {
                RegisterDisplayNameError = "Display name is too long.";
                isValid = false;
            }

            return isValid;
        }

        private static bool IsValidEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            try
            {
                _ = new MailAddress(value.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void OnAuthStateChanged(object? sender, EventArgs e)
        {
            SyncFromAuthService();
            await LoadAvatarAsync(CurrentAvatarUrl);
        }

        private void SyncFromAuthService()
        {
            var user = authService.CurrentUser;
            IsAuthenticated = authService.IsAuthenticated;
            CurrentDisplayName = user?.Name ?? "Guest";
            CurrentEmail = user?.Email ?? "Browse without signing in";
            CurrentRole = user?.Role ?? "GUEST";
            CurrentUserId = user?.Id ?? "GUEST";
            CurrentAvatarUrl = user?.AvatarUrl ?? string.Empty;
            SessionState = IsAuthenticated ? "Signed in" : "Guest";
            TokenSummary = TokenManager.HasStoredTokens()
                ? "Your session is stored on this device."
                : "No active session on this device.";

            if (!IsAuthenticated)
            {
                Interlocked.Increment(ref avatarLoadVersion);
                AvatarImage = null;
            }
        }

        private async Task LoadAvatarAsync(string avatarUrl)
        {
            var loadVersion = Interlocked.Increment(ref avatarLoadVersion);

            if (string.IsNullOrWhiteSpace(avatarUrl))
            {
                AvatarImage = null;
                return;
            }

            try
            {
                using var stream = await AvatarHttpClient.GetStreamAsync(avatarUrl);
                using var memory = new MemoryStream();
                await stream.CopyToAsync(memory);

                if (loadVersion != Volatile.Read(ref avatarLoadVersion) ||
                    !string.Equals(avatarUrl, CurrentAvatarUrl, StringComparison.Ordinal))
                {
                    return;
                }

                memory.Position = 0;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = memory;
                bitmap.EndInit();
                bitmap.Freeze();

                if (loadVersion != Volatile.Read(ref avatarLoadVersion) ||
                    !string.Equals(avatarUrl, CurrentAvatarUrl, StringComparison.Ordinal))
                {
                    return;
                }

                AvatarImage = bitmap;
            }
            catch
            {
                if (loadVersion == Volatile.Read(ref avatarLoadVersion))
                {
                    AvatarImage = null;
                }
            }
        }

        private static string FormatRole(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unknown";
            }

            return string.Join(
                " ",
                value.Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
        }

        private void ClearErrors()
        {
            LoginEmailError = string.Empty;
            LoginPasswordError = string.Empty;
            RegisterEmailError = string.Empty;
            RegisterPasswordError = string.Empty;
            RegisterDisplayNameError = string.Empty;
        }

        private void ClearStatus()
        {
            statusDismissCts?.Cancel();
            statusDismissCts?.Dispose();
            statusDismissCts = null;
            StatusMessage = string.Empty;
        }

        private void SetStatus(string message, bool isError)
        {
            statusDismissCts?.Cancel();
            statusDismissCts?.Dispose();
            statusDismissCts = null;

            StatusMessage = message;
            IsStatusError = isError;

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var dismissToken = new CancellationTokenSource();
            statusDismissCts = dismissToken;
            _ = DismissStatusAsync(dismissToken, isError ? TimeSpan.FromSeconds(4.8) : TimeSpan.FromSeconds(3.2));
        }

        private async Task DismissStatusAsync(CancellationTokenSource dismissToken, TimeSpan duration)
        {
            try
            {
                await Task.Delay(duration, dismissToken.Token);

                if (ReferenceEquals(statusDismissCts, dismissToken))
                {
                    StatusMessage = string.Empty;
                    statusDismissCts.Dispose();
                    statusDismissCts = null;
                }
            }
            catch (TaskCanceledException)
            {
                if (ReferenceEquals(statusDismissCts, dismissToken))
                {
                    statusDismissCts = null;
                }
            }
        }

        private void ClearFormInputs()
        {
            LoginEmail = string.Empty;
            LoginPassword = string.Empty;
            RegisterEmail = string.Empty;
            RegisterPassword = string.Empty;
            RegisterDisplayName = string.Empty;
        }

    }
}
