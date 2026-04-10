using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Royal_Blueberry_Dictionary.Model.Auth;
using Royal_Blueberry_Dictionary.Service;
using System.Net.Mail;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.ViewModel
{
    public partial class WelcomeWindowViewModel : ObservableObject, IDisposable
    {
        private static readonly Brush SuccessBackgroundBrush = new SolidColorBrush(Color.FromRgb(236, 253, 245));
        private static readonly Brush SuccessBorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
        private static readonly Brush SuccessForegroundBrush = new SolidColorBrush(Color.FromRgb(4, 120, 87));
        private static readonly Brush ErrorBackgroundBrush = new SolidColorBrush(Color.FromRgb(254, 242, 242));
        private static readonly Brush ErrorBorderBrush = new SolidColorBrush(Color.FromRgb(248, 113, 113));
        private static readonly Brush ErrorForegroundBrush = new SolidColorBrush(Color.FromRgb(185, 28, 28));

        private readonly AuthService authService;
        private readonly ApplicationFlowService applicationFlowService;
        private bool initialized;
        private CancellationTokenSource? statusDismissCts;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAuthMode))]
        [NotifyPropertyChangedFor(nameof(ChoiceTitle))]
        [NotifyPropertyChangedFor(nameof(ChoiceSubtitle))]
        private bool isChoiceMode = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRegisterMode))]
        [NotifyPropertyChangedFor(nameof(AuthTitle))]
        [NotifyPropertyChangedFor(nameof(AuthSubtitle))]
        [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
        private bool isLoginMode = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
        private bool isSubmitting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GoogleButtonText))]
        private bool isGoogleBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ChoiceTitle))]
        [NotifyPropertyChangedFor(nameof(ChoiceSubtitle))]
        [NotifyPropertyChangedFor(nameof(ContinueButtonText))]
        [NotifyPropertyChangedFor(nameof(LoginChoiceText))]
        [NotifyPropertyChangedFor(nameof(RegisterChoiceText))]
        private bool hasActiveSession;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContinueButtonText))]
        private bool isRestoringSession;

        [ObservableProperty]
        private string currentDisplayName = "Guest";

        [ObservableProperty]
        private string currentEmail = "Browse without signing in";

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
        [NotifyPropertyChangedFor(nameof(StatusBackground))]
        [NotifyPropertyChangedFor(nameof(StatusBorderBrush))]
        [NotifyPropertyChangedFor(nameof(StatusForeground))]
        private bool isStatusError = true;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        public WelcomeWindowViewModel(AuthService authService, ApplicationFlowService applicationFlowService)
        {
            this.authService = authService;
            this.applicationFlowService = applicationFlowService;
            authService.AuthStateChanged += OnAuthStateChanged;
            SyncState();
        }

        public bool IsAuthMode => !IsChoiceMode;

        public bool IsRegisterMode => !IsLoginMode;

        public string ChoiceTitle => HasActiveSession ? "Welcome back" : "Choose how to continue";

        public string ChoiceSubtitle => HasActiveSession
            ? "A saved session was found on this device. Continue with it, switch account, or enter as guest."
            : "Sign in, create an account, or continue as a guest. Your main workspace opens after you finish here.";

        public string ContinueButtonText => IsRestoringSession
            ? "Checking your saved session..."
            : $"Continue with {CurrentDisplayName}";

        public string LoginChoiceText => HasActiveSession ? "Use another account" : "Sign in";

        public string RegisterChoiceText => HasActiveSession ? "Create a new account" : "Create account";

        public string AuthTitle => IsLoginMode ? "Sign in" : "Create account";

        public string AuthSubtitle => IsLoginMode
            ? "Use your Blueberry account credentials or continue with Google."
            : "Create an account to store your session on this device and enter the main app after success.";

        public string SubmitButtonText => IsSubmitting
            ? (IsLoginMode ? "Signing in..." : "Creating account...")
            : (IsLoginMode ? "Sign in" : "Create account");

        public string GoogleButtonText => IsGoogleBusy ? "Connecting to Google..." : "Continue with Google";

        public Brush StatusBackground => IsStatusError ? ErrorBackgroundBrush : SuccessBackgroundBrush;

        public Brush StatusBorderBrush => IsStatusError ? ErrorBorderBrush : SuccessBorderBrush;

        public Brush StatusForeground => IsStatusError ? ErrorForegroundBrush : SuccessForegroundBrush;

        public async Task InitializeAsync()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            await RestoreSessionIfNeededAsync();
        }

        [RelayCommand]
        private void OpenLogin()
        {
            IsChoiceMode = false;
            IsLoginMode = true;
            ClearErrors();
            ClearStatus();
        }

        [RelayCommand]
        private void OpenRegister()
        {
            IsChoiceMode = false;
            IsLoginMode = false;
            ClearErrors();
            ClearStatus();
        }

        [RelayCommand]
        private void BackToChoice()
        {
            IsChoiceMode = true;
            ClearFormInputs();
            ClearErrors();
            ClearStatus();
            SyncState();
        }

        [RelayCommand]
        private void ContinueWithSavedSession()
        {
            if (!HasActiveSession || IsRestoringSession)
            {
                return;
            }

            applicationFlowService.EnterMainApp();
        }

        [RelayCommand]
        private void ContinueAsGuest()
        {
            authService.ContinueAsGuest();
            SyncState();
            applicationFlowService.EnterMainApp();
        }

        [RelayCommand]
        private async Task SubmitAuthAsync()
        {
            if (IsSubmitting || IsGoogleBusy || IsRestoringSession)
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

                    await CompleteAuthAsync(result, "Signed in successfully.");
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

                await CompleteAuthAsync(result, "Account created successfully.");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        [RelayCommand]
        private async Task SignInWithGoogleAsync()
        {
            if (IsSubmitting || IsGoogleBusy || IsRestoringSession)
            {
                return;
            }

            ClearErrors();
            ClearStatus();

            IsGoogleBusy = true;
            try
            {
                var result = await authService.LoginWithGoogleAsync();
                await CompleteAuthAsync(result, "Google sign-in completed.");
            }
            finally
            {
                IsGoogleBusy = false;
            }
        }

        public void Dispose()
        {
            ClearStatus();
            authService.AuthStateChanged -= OnAuthStateChanged;
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

        private async Task CompleteAuthAsync(AuthResult result, string successMessage)
        {
            if (!result.IsSuccess)
            {
                ApplyValidationErrors(result.ValidationErrors);
                SetStatus(result.ErrorMessage ?? "Authentication failed.", true);
                SyncState();
                return;
            }

            ClearFormInputs();
            ClearErrors();
            SyncState();
            SetStatus(successMessage, false);
            await Task.Delay(120);
            applicationFlowService.EnterMainApp();
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
