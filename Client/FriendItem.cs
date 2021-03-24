using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public class FriendItem
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public string ConverastionID { get; set; }
        public int NotificationsAmount { get; set; }

        public FriendItem(string name, bool active, string converastionID, int notificationsAmount = 0) {
            Name = name;
            Active = active;
            ConverastionID = converastionID;
            NotificationsAmount = notificationsAmount;
        }
    }
}
