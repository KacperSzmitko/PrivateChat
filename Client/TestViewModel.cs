using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Client
{
    class TestViewModel
    {
        public ObservableCollection<FriendItem> Friends {
            get {
                return new ObservableCollection<FriendItem>() {
                    new FriendItem("test2", true, 0),
                    new FriendItem("test3", true, 1),
                    new FriendItem("test3", false, 7)
                };
            }
        }

        public ObservableCollection<MessageItem> Messages {
            get {
                return new ObservableCollection<MessageItem>() {
                    new MessageItem("test1", "Moja wiadomość testowa", DateTime.Parse("24.03.2021 15:30"), true),
                    new MessageItem("test2", "Wiadomość testowa znajomego", DateTime.Parse("24.03.2021 15:33"), false),
                    new MessageItem("test2", "Dodatkowa Wiadomość testowa znajomego", DateTime.Parse("24.03.2021 15:39"), false),
                    new MessageItem("test1", "ok", DateTime.Parse("24.03.2021 16:12"), true),
                    new MessageItem("test2", "Długa wiadomość testowa znajomego, która jest bardzo długa i posiada dużo tekstu w sobie", DateTime.Parse("24.03.2021 17:01"), false),
                    new MessageItem("test1", "bardzo ładnie :)", DateTime.Parse("24.03.2021 16:38"), true, MessageStatuses.MESSAGE_RECEIVED_SERVER)
                };
            }
        }
    }
}
