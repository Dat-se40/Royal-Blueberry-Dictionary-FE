using Royal_Blueberry_Dictionary.Database;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;

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
            serviceCollection.AddDbContext<AppDbContext>();

            // Repositories
            serviceCollection.AddScoped<Repository.Interface.IWordEntryRepository, Repository.Implement.WordEntryRepository>();

            serviceProvider = serviceCollection.BuildServiceProvider();

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
