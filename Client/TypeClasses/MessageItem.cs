namespace Client
{
    public class MessageItem
    {
        public string Username { get; set; }
        public string MessageText { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public bool MyMessage { get; set; }

        public MessageItem(string username, string messageText, string date, string time, bool myMessage) {
            Username = username;
            MessageText = messageText;
            Date = date;
            Time = time;
            MyMessage = myMessage;
        }
    }
}
