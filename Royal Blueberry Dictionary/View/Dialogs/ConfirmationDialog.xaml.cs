using System.Linq;
using System.Windows;

namespace Royal_Blueberry_Dictionary.View.Dialogs
{
    public partial class ConfirmationDialog : Window
    {
        public string TitleText { get; }

        public string HeadingText { get; }

        public string MessageText { get; }

        public string PrimaryButtonText { get; }

        public string SecondaryButtonText { get; }

        public bool IsDanger { get; }

        public ConfirmationDialog(
            string titleText,
            string headingText,
            string messageText,
            string primaryButtonText,
            string secondaryButtonText,
            bool isDanger = true)
        {
            InitializeComponent();

            TitleText = string.IsNullOrWhiteSpace(titleText) ? "Confirm action" : titleText;
            HeadingText = string.IsNullOrWhiteSpace(headingText) ? TitleText : headingText;
            MessageText = messageText;
            PrimaryButtonText = string.IsNullOrWhiteSpace(primaryButtonText) ? "Confirm" : primaryButtonText;
            SecondaryButtonText = string.IsNullOrWhiteSpace(secondaryButtonText) ? "Cancel" : secondaryButtonText;
            IsDanger = isDanger;

            DataContext = this;
            Loaded += ConfirmationDialog_Loaded;
        }

        public static bool Show(
            string titleText,
            string headingText,
            string messageText,
            string primaryButtonText,
            string secondaryButtonText,
            bool isDanger = true,
            Window? owner = null)
        {
            var dialog = new ConfirmationDialog(
                titleText,
                headingText,
                messageText,
                primaryButtonText,
                secondaryButtonText,
                isDanger);

            var resolvedOwner = owner
                ?? Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
                ?? Application.Current.MainWindow;

            if (resolvedOwner != null && resolvedOwner != dialog)
            {
                dialog.Owner = resolvedOwner;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            return dialog.ShowDialog() == true;
        }

        private void ConfirmationDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsDanger)
            {
                PrimaryActionButton.Style = (Style)FindResource("SearchButtonStyle");
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
