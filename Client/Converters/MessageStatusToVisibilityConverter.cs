using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Client.Converters
{
    [ValueConversion(typeof(MessageStatuses), typeof(string))]
    class MessageStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((MessageStatuses)value != MessageStatuses.NOT_LAST_MESSAGE) return "Visible";
            return "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((string)value == "Visible") return MessageStatuses.MESSAGE_SENT;
            return false;
        }
    }
}
