using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Client.Converters
{
    [ValueConversion(typeof(MessageStatuses), typeof(string))]
    class MessageStatusToSymbolColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((MessageStatuses)value == MessageStatuses.MESSAGE_RECEIVED_FRIEND) return "#00ff00";
            return "#DADDE7";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((string)value == "#00ff00") return MessageStatuses.MESSAGE_RECEIVED_FRIEND;
            return MessageStatuses.NOT_LAST_MESSAGE;
        }
    }
}
