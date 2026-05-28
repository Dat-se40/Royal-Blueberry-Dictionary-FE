using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.ViewModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = App.serviceProvider.GetRequiredService<HomePageViewModel>();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
