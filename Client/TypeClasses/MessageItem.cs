using System;

namespace Client
{
    public class MessageItem
    {
        public string Username { get; set; }
        public string MessageText { get; set; }
        public DateTime DateTime { get; set; }
        public bool MyMessage { get; set; }

        public MessageItem(string username, string messageText, DateTime dateTime, bool myMessage) {
            Username = username;
            MessageText = messageText;
            DateTime = dateTime;
            MyMessage = myMessage;
        }
    }
}
