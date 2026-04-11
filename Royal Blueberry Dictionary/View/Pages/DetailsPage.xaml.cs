using BlueBerryDictionary.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Royal_Blueberry_Dictionary.View.Pages
{
    /// <summary>
    /// Interaction logic for DetailsPage.xaml
    /// </summary>
    public partial class DetailsPage : Page
    {
        public DetailsPage()
        {
            InitializeComponent();
            
        }

        private void PlayAudioUS_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DetailsPageViewModel vm && vm.PlayAudioUsCommand.CanExecute(null))
                vm.PlayAudioUsCommand.Execute(null);
        }

        private void PlayAudioUK_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DetailsPageViewModel vm && vm.PlayAudioUkCommand.CanExecute(null))
                vm.PlayAudioUkCommand.Execute(null);
        }
    }
}
