using Royal_Blueberry_Dictionary.Database;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media.Animation;

namespace Royal_Blueberry_Dictionary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() 
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }   
        }
    }

}
