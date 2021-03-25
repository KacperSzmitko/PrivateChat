using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Client
{
    class TestViewModel
    {
        public ObservableCollection<MessageItem> Conversation {
            get {
                return new ObservableCollection<MessageItem>() { 
                    new MessageItem("test1", "Moja wiadomość testowa", "24.03.2021", "15:36", true), 
                    new MessageItem("test2", "Wiadomość testowa znajomego", "24.03.2021", "15:38", false),
                    new MessageItem("test2", "Dodatkowa Wiadomość testowa znajomego", "24.03.2021", "15:39", false),
                    new MessageItem("test1", "ok", "24.03.2021", "16:12", true),
                    new MessageItem("test2", "Długa wiadomość testowa znajomego, która jest bardzo długa i posiada dużo tekstu w sobie", "24.03.2021", "16:56", false)
                };
            }
        }
    }
}
