using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.Service;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    public partial class HomePage : Page
    {
        private readonly NavigationService _navigationService;

        public HomePage()
        {
            InitializeComponent();
            _navigationService = App.serviceProvider.GetRequiredService<NavigationService>();
        }

        private void ButtnNavigate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            _navigationService.NavigateByTag(button.Tag?.ToString());
        }
    }
}