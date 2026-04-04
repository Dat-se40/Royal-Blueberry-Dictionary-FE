using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Royal_Blueberry_Dictionary.Converters
{
    /// <summary>Trả về tối đa N phần tử đầu từ IEnumerable (mặc định 5).</summary>
    public class TakeFirstNEnumerableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var n = 5;
            if (parameter != null && int.TryParse(parameter.ToString(), out var parsed) && parsed > 0)
                n = parsed;

            if (value is IEnumerable<string> strings)
                return strings.Where(s => !string.IsNullOrWhiteSpace(s)).Take(n).ToList();

            if (value is string)
                return new List<string>();

            if (value is IEnumerable seq)
            {
                var list = new List<string>();
                foreach (var item in seq)
                {
                    if (item is string s && !string.IsNullOrWhiteSpace(s))
                        list.Add(s);
                    if (list.Count >= n)
                        break;
                }
                return list;
            }

            return new List<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
