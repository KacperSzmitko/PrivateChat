using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Client.Converters
{
    [ValueConversion(typeof(MessageStatuses), typeof(string))]
    class MessageStatusToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((MessageStatuses)value == MessageStatuses.MESSAGE_SENT) return ">";
            if ((MessageStatuses)value == MessageStatuses.MESSAGE_RECEIVED_SERVER) return "✓";
            if ((MessageStatuses)value == MessageStatuses.MESSAGE_RECEIVED_FRIEND) return "✓";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((string)value == ">") return MessageStatuses.MESSAGE_SENT;
            if ((string)value == "✓") return MessageStatuses.MESSAGE_RECEIVED_SERVER;
            if ((string)value == "✓") return MessageStatuses.MESSAGE_RECEIVED_FRIEND;
            return MessageStatuses.NOT_LAST_MESSAGE;
        }
    }
}
