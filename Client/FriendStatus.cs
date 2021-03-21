using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public class FriendStatus
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public int NotificationsAmount { get; set; }

        public FriendStatus(string name, bool active, int notificationsAmount = 0) {
            Name = name;
            Active = active;
            NotificationsAmount = notificationsAmount;
        }
    }
}
