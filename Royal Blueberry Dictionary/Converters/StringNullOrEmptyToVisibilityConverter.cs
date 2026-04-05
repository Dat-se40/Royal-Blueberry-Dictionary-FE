using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Royal_Blueberry_Dictionary.Converters
{
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public bool CollapseWhenNullOrEmpty { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            bool isNullOrEmpty = string.IsNullOrEmpty(s);

            var visibilityWhenEmpty = CollapseWhenNullOrEmpty ? Visibility.Collapsed : Visibility.Hidden;
            return isNullOrEmpty ? visibilityWhenEmpty : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
