using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateTimeToTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((DateTime)value).ToString("t");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DateTime.Parse((string)value);
        }
    }
}
