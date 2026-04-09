using BlueBerryDictionary.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Config;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using Royal_Blueberry_Dictionary.View.Pages;
using Royal_Blueberry_Dictionary.ViewModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.WebSockets;
using System.Windows;
using System.Windows.Media.Animation;
using AppThemeMode = Royal_Blueberry_Dictionary.Service.ThemeMode;
namespace Royal_Blueberry_Dictionary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider serviceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<AppDbContext>(options =>
                                options.UseSqlite("Data Source=blueberry.db")
            );

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json",optional: false, reloadOnChange : true).Build();
            var apiSettings = new ApiSettings();
            config.GetSection("ApiSettings").Bind(apiSettings);
            serviceCollection.AddSingleton(apiSettings);

            // Repositories
            serviceCollection.AddScoped<Repository.Interface.IWordEntryRepository, Repository.Implement.WordEntryRepository>();
            serviceCollection.AddScoped<Repository.Interface.ITagRepository, Repository.Implement.TagRepository>();
            // ApiClient
            serviceCollection.AddSingleton<IBackendApiClient, BackendApiClient>();
            //Service 
            serviceCollection.AddScoped<Service.SearchService>();
            serviceCollection.AddScoped<Service.PackageService>();
            serviceCollection.AddScoped<Service.NavigationService>();
            serviceCollection.AddScoped<Service.TagService>();
            serviceCollection.AddScoped<Service.WordService>();
            serviceCollection.AddSingleton<Service.ThemeManager>();
            serviceCollection.AddSingleton(provider => Service.SettingsService.Instance);

            // Views
            serviceCollection.AddTransient<DetailsPage>();
            serviceCollection.AddTransient<HistoryPage>();
            serviceCollection.AddTransient<FavouriteWordsPage>();
            serviceCollection.AddTransient<MyWordsPage>();
            serviceCollection.AddTransient<HomePage>();
            serviceCollection.AddTransient<SettingsPage>(); 
            // View Models
            serviceCollection.AddScoped<DetailsPageViewModel>();  
            serviceCollection.AddScoped<SearchViewModel >();
            serviceCollection.AddScoped<HistoryPageViewModel>();
            serviceCollection.AddScoped<FavouriteWordsPageViewModel>();
            serviceCollection.AddScoped<MyWordsPageViewModel>();
            serviceCollection.AddScoped<ViewModel.SettingsPageViewModel>();
            serviceProvider = serviceCollection.BuildServiceProvider();

            var settingsService = serviceProvider.GetRequiredService<Service.SettingsService>();
            var themeManager = serviceProvider.GetRequiredService<Service.ThemeManager>();

            // Apply saved settings
            var settings = settingsService.CurrentSettings;

            // Apply theme mode
            var themeMode = settings.ThemeMode switch
            {
                "Light" => Service.ThemeMode.Light,
                "Dark" => Service.ThemeMode.Dark,
                "System" => Service.ThemeMode.System,
                _ => Service.ThemeMode.Light
            };
            themeManager.SetThemeMode(themeMode);

            // Apply color theme
            if (!string.IsNullOrEmpty(settings.ColorTheme) && settings.ColorTheme != "default")
            {
                if (settings.ColorTheme == "custom" && settings.CustomColorTheme != null)
                {
                    themeManager.ApplyCustomColorTheme(
                        settings.CustomColorTheme.Primary,
                        settings.CustomColorTheme.Secondary,
                        settings.CustomColorTheme.Accent
                    );
                }
                else
                {
                    themeManager.ApplyColorTheme(settings.ColorTheme);
                }
            }

            // Apply font
            if (!string.IsNullOrEmpty(settings.FontFamily) && settings.FontFamily != "Segoe UI")
            {
                var fontFamily = new System.Windows.Media.FontFamily(settings.FontFamily);
                Application.Current.Resources["AppFontFamily"] = fontFamily;
                Application.Current.Resources["AppFontSize"] = settings.FontSize;
            }

            System.Diagnostics.Debug.WriteLine("✅ App settings applied at startup");

            // Ensure database is created
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (serviceProvider != null)
            {
                serviceProvider.Dispose();
            }
        }
    } 

    }
