using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Config;
using Royal_Blueberry_Dictionary.Database;
using Royal_Blueberry_Dictionary.Service.ApiClient;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.WebSockets;
using System.Windows;
using System.Windows.Media.Animation;

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

            // ApiClient
            serviceCollection.AddSingleton<IBackendApiClient, BackendApiClient>();
            //Service 
            serviceCollection.AddScoped<Service.SearchService>();
            serviceCollection.AddScoped<Service.PackageService>()
                ; 
            serviceProvider = serviceCollection.BuildServiceProvider();
            

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
