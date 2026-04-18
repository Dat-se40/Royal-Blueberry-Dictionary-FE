using Royal_Blueberry_Dictionary.Service;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Royal_Blueberry_Dictionary.View.Dialogs.Settings
{
    public partial class CustomThemeDialog : Window
    {
        #region Properties

        public Color PrimaryColor { get; private set; }
        public Color SecondaryColor { get; private set; }
        public Color AccentColor { get; private set; }

        private readonly ThemeManager _themeManager;

        #endregion

        #region Constructor

        public CustomThemeDialog(ThemeManager themeManager)
        {
            InitializeComponent();

            _themeManager = themeManager;

            PrimaryColor = (Color)ColorConverter.ConvertFromString("#667EEA");
            SecondaryColor = (Color)ColorConverter.ConvertFromString("#8B9DFF");
            AccentColor = (Color)ColorConverter.ConvertFromString("#F093FB");

            ApplyGlobalFont();
            LoadCurrentCustomColors();

            PrimaryColorPicker.SelectedColor = PrimaryColor;
            SecondaryColorPicker.SelectedColor = SecondaryColor;
            AccentColorPicker.SelectedColor = AccentColor;

            UpdateColorUI();
        }

        #endregion

        #region Private Methods

        private void LoadCurrentCustomColors()
        {
            // TODO: nếu sau này load từ settings thì gán lại ở đây
        }

        private void ApplyGlobalFont()
        {
            try
            {
                if (Application.Current.Resources.Contains("AppFontFamily"))
                {
                    this.FontFamily = (FontFamily)Application.Current.Resources["AppFontFamily"];
                }

                if (Application.Current.Resources.Contains("AppFontSize"))
                {
                    this.FontSize = (double)Application.Current.Resources["AppFontSize"];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Apply font error: {ex.Message}");
            }
        }

        private void UpdateColorUI()
        {
            if (PrimaryColorButton != null)
                PrimaryColorButton.Background = new SolidColorBrush(PrimaryColor);

            if (SecondaryColorButton != null)
                SecondaryColorButton.Background = new SolidColorBrush(SecondaryColor);

            if (AccentColorButton != null)
                AccentColorButton.Background = new SolidColorBrush(AccentColor);

            if (PrimaryPreview != null)
                PrimaryPreview.Background = new SolidColorBrush(PrimaryColor);

            if (SecondaryPreview != null)
                SecondaryPreview.Background = new SolidColorBrush(SecondaryColor);

            if (AccentPreview != null)
                AccentPreview.Background = new SolidColorBrush(AccentColor);

            if (PrimaryHexText != null)
                PrimaryHexText.Text = ToHex(PrimaryColor);

            if (SecondaryHexText != null)
                SecondaryHexText.Text = ToHex(SecondaryColor);

            if (AccentHexText != null)
                AccentHexText.Text = ToHex(AccentColor);
        }

        private string ToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private void OpenColorPicker(Control picker)
        {
            picker.Focus();

            // Giả lập click để mở popup dropdown của ColorPicker
            var args = new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent);
            picker.RaiseEvent(args);
        }

        #endregion

        #region Color Button Events

        private void PrimaryColorButton_Click(object sender, RoutedEventArgs e)
        {
            OpenColorPicker(PrimaryColorPicker);
        }

        private void SecondaryColorButton_Click(object sender, RoutedEventArgs e)
        {
            OpenColorPicker(SecondaryColorPicker);
        }

        private void AccentColorButton_Click(object sender, RoutedEventArgs e)
        {
            OpenColorPicker(AccentColorPicker);
        }

        #endregion

        #region ColorPicker Changed Events

        private void PrimaryColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            PrimaryColor = e.NewValue ?? PrimaryColor;
            UpdateColorUI();
        }

        private void SecondaryColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SecondaryColor = e.NewValue ?? SecondaryColor;
            UpdateColorUI();
        }

        private void AccentColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            AccentColor = e.NewValue ?? AccentColor;
            UpdateColorUI();
        }

        #endregion

        #region Action Events

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            _themeManager.ApplyCustomColorTheme(PrimaryColor, SecondaryColor, AccentColor);

            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}
