using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    [ValueConversion(typeof(bool), typeof(int))]
    public class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((bool)value == true) return 1;
            else return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((int)value >= 1) return true;
            else return false;
        }
    }
}