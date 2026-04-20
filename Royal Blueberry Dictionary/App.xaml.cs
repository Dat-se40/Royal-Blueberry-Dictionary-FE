using BlueBerryDictionary.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Config;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using Royal_Blueberry_Dictionary.View.Pages;
using Royal_Blueberry_Dictionary.ViewModel;
using System.IO;
using System.Windows;

namespace Royal_Blueberry_Dictionary
{
    public partial class App : Application
    {
        public static ServiceProvider serviceProvider { get; private set; } = null!;

        private static string userId = "GUEST";

        public static string UserId
        {
            get => userId;
            set => userId = !string.IsNullOrWhiteSpace(value) ? value : "GUEST";
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            var dbFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "RoyalBlueberryDictionary"
);
            Directory.CreateDirectory(dbFolder);
            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={Path.Combine(dbFolder, "blueberry.db")}"));
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var apiSettings = new ApiSettings();
            config.GetSection("ApiSettings").Bind(apiSettings);
            serviceCollection.AddSingleton(apiSettings);

            serviceCollection.AddScoped<Repository.Interface.IWordEntryRepository, Repository.Implement.WordEntryRepository>();
            serviceCollection.AddScoped<Repository.Interface.ITagRepository, Repository.Implement.TagRepository>();
            serviceCollection.AddSingleton<Service.GameLogService>();
            serviceCollection.AddSingleton<IBackendApiClient, BackendApiClient>();
            serviceCollection.AddSingleton<Service.OfflinePackageCacheService>();

            serviceCollection.AddScoped<Service.SearchService>();
            serviceCollection.AddScoped<Service.TagService>();
            serviceCollection.AddScoped<Service.PackageService>();
            serviceCollection.AddScoped<Service.NavigationService>();
            serviceCollection.AddScoped<Service.WordService>();
            serviceCollection.AddSingleton<Service.ThemeManager>();
            serviceCollection.AddSingleton(_ => Service.SettingsService.Instance);
            serviceCollection.AddSingleton<Service.AuthService>();
            serviceCollection.AddSingleton<Service.ApplicationFlowService>();

            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddTransient<WelcomeWindow>();

            serviceCollection.AddTransient<DetailsPage>();
            serviceCollection.AddTransient<HistoryPage>();
            serviceCollection.AddTransient<FavouriteWordsPage>();
            serviceCollection.AddTransient<MyWordsPage>();
            serviceCollection.AddTransient<HomePage>();
            serviceCollection.AddTransient<SettingsPage>();
            serviceCollection.AddTransient<AccountPage>();
            serviceCollection.AddTransient<GamePage>();
            serviceCollection.AddTransient<OfflinePackagesPage>();

            serviceCollection.AddScoped<DetailsPageViewModel>();
            serviceCollection.AddScoped<SearchViewModel>();
            serviceCollection.AddScoped<HistoryPageViewModel>();
            serviceCollection.AddScoped<FavouriteWordsPageViewModel>();
            serviceCollection.AddScoped<MyWordsPageViewModel>();
            serviceCollection.AddScoped<SettingsPageViewModel>();
            serviceCollection.AddScoped<AccountPageViewModel>();
            serviceCollection.AddScoped<GameViewModel>();
            serviceCollection.AddTransient<WelcomeWindowViewModel>();
            serviceCollection.AddTransient<GameViewModel>();
            serviceCollection.AddScoped<OfflinePackagesPageViewModel>();

            serviceProvider = serviceCollection.BuildServiceProvider();

            var settingsService = serviceProvider.GetRequiredService<Service.SettingsService>();
            var themeManager = serviceProvider.GetRequiredService<Service.ThemeManager>();
            var settings = settingsService.CurrentSettings;

            var themeMode = settings.ThemeMode switch
            {
                "Light" => Service.ThemeMode.Light,
                "Dark" => Service.ThemeMode.Dark,
                "System" => Service.ThemeMode.System,
                _ => Service.ThemeMode.Light
            };
            themeManager.SetThemeMode(themeMode);

            if (!string.IsNullOrEmpty(settings.ColorTheme) && settings.ColorTheme != "default")
            {
                if (settings.ColorTheme == "custom" && settings.CustomColorTheme != null)
                {
                    themeManager.ApplyCustomColorTheme(
                        settings.CustomColorTheme.Primary,
                        settings.CustomColorTheme.Secondary,
                        settings.CustomColorTheme.Accent);
                }
                else
                {
                    themeManager.ApplyColorTheme(settings.ColorTheme);
                }
            }

            if (!string.IsNullOrEmpty(settings.FontFamily) && settings.FontFamily != "Segoe UI")
            {
                var fontFamily = new System.Windows.Media.FontFamily(settings.FontFamily);
                Current.Resources["AppFontFamily"] = fontFamily;
                Current.Resources["AppFontSize"] = settings.FontSize;
            }

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            ShutdownMode = ShutdownMode.OnLastWindowClose;
            serviceProvider.GetRequiredService<Service.ApplicationFlowService>().ShowWelcomeWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            serviceProvider.Dispose();
        }
    }
}
