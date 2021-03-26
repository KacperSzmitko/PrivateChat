using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Client.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    class BoolToSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((bool)value == true) return "Right";
            return "Left";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((string)value == "Right") return true;
            return false;
        }
    }
}
