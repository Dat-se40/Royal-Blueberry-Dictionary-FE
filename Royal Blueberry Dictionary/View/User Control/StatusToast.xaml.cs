using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Royal_Blueberry_Dictionary.View.User_Control
{
    public partial class StatusToast : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(StatusToast),
                new PropertyMetadata(string.Empty, OnMessageChanged));

        public static readonly DependencyProperty IsErrorProperty =
            DependencyProperty.Register(
                nameof(IsError),
                typeof(bool),
                typeof(StatusToast),
                new PropertyMetadata(false));

        public StatusToast()
        {
            InitializeComponent();
            Loaded += StatusToast_Loaded;
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public bool IsError
        {
            get => (bool)GetValue(IsErrorProperty);
            set => SetValue(IsErrorProperty, value);
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatusToast toast)
            {
                toast.UpdateToastState();
            }
        }

        private void StatusToast_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateToastState(initial: true);
        }

        private void UpdateToastState(bool initial = false)
        {
            if (ToastCard == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                HideToast(initial);
                return;
            }

            ShowToast(initial);
        }

        private void ShowToast(bool initial)
        {
            ToastCard.BeginAnimation(OpacityProperty, null);
            ((TranslateTransform)ToastCard.RenderTransform).BeginAnimation(TranslateTransform.YProperty, null);

            ToastCard.Visibility = Visibility.Visible;
            ToastCard.Opacity = initial ? 1 : 0;
            ((TranslateTransform)ToastCard.RenderTransform).Y = initial ? 0 : -10;

            if (initial)
            {
                return;
            }

            var duration = TimeSpan.FromMilliseconds(180);
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            ToastCard.BeginAnimation(
                OpacityProperty,
                new DoubleAnimation(0, 1, duration) { EasingFunction = easing });

            ((TranslateTransform)ToastCard.RenderTransform).BeginAnimation(
                TranslateTransform.YProperty,
                new DoubleAnimation(-10, 0, duration) { EasingFunction = easing });
        }

        private void HideToast(bool initial)
        {
            ToastCard.BeginAnimation(OpacityProperty, null);
            ((TranslateTransform)ToastCard.RenderTransform).BeginAnimation(TranslateTransform.YProperty, null);

            if (initial || ToastCard.Visibility != Visibility.Visible)
            {
                ToastCard.Visibility = Visibility.Collapsed;
                ToastCard.Opacity = 0;
                ((TranslateTransform)ToastCard.RenderTransform).Y = -10;
                return;
            }

            var duration = TimeSpan.FromMilliseconds(150);
            var easing = new CubicEase { EasingMode = EasingMode.EaseIn };
            var fade = new DoubleAnimation(ToastCard.Opacity, 0, duration) { EasingFunction = easing };
            fade.Completed += (_, _) =>
            {
                ToastCard.BeginAnimation(OpacityProperty, null);
                ((TranslateTransform)ToastCard.RenderTransform).BeginAnimation(TranslateTransform.YProperty, null);
                ToastCard.Opacity = 0;
                ((TranslateTransform)ToastCard.RenderTransform).Y = -10;
                ToastCard.Visibility = Visibility.Collapsed;
            };

            ToastCard.BeginAnimation(OpacityProperty, fade);
            ((TranslateTransform)ToastCard.RenderTransform).BeginAnimation(
                TranslateTransform.YProperty,
                new DoubleAnimation(0, -8, duration) { EasingFunction = easing });
        }
    }
}
