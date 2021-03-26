namespace Client
{
    public class FriendItem
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public int NotificationsAmount { get; set; }

        public FriendItem(string name, bool active, int notificationsAmount = 0) {
            Name = name;
            Active = active;
            NotificationsAmount = notificationsAmount;
        }
    }
}
