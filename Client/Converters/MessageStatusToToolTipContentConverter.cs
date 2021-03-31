using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Client.Converters
{
    [ValueConversion(typeof(MessageStatuses), typeof(string))]
    class MessageStatusToToolTipContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((MessageStatuses)value == MessageStatuses.MESSAGE_SENT) return "Wysyłanie...";
            if ((MessageStatuses)value == MessageStatuses.MESSAGE_RECEIVED_SERVER) return "Wysłano";
            if ((MessageStatuses)value == MessageStatuses.MESSAGE_RECEIVED_FRIEND) return "Odebrano przez znajomego";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((string)value == "Wysyłanie...") return MessageStatuses.MESSAGE_SENT;
            if ((string)value == "Wysłano") return MessageStatuses.MESSAGE_RECEIVED_SERVER;
            if ((string)value == "Odebrano przez znajomego") return MessageStatuses.MESSAGE_RECEIVED_FRIEND;
            return MessageStatuses.NOT_LAST_MESSAGE;
        }
    }
}
