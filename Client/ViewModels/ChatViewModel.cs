using Client.Models;
using Newtonsoft.Json;
using Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private ChatModel model;

        private readonly string username;

        private ObservableCollection<Friend> friendsList;

        public string Username { get { return username; } }

        public ObservableCollection<Friend> FriendsList { get { return friendsList; } }

        public ChatViewModel(ServerConnection connection, Navigator navigator, string username) : base(connection, navigator) {
            this.model = new ChatModel(this.connection);
            this.username = username;
            friendsList = new ObservableCollection<Friend>(JsonConvert.DeserializeObject<List<Friend>>(this.model.GetFriendsJSON()));

        }
    }
}
