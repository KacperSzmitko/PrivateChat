using Shared;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Client.Models
{
    public class ChatModel : BaseModel
    {
        private string username;
        private string invitationUsername;
        private List<Friend> friendsList;

        public string Username { get { return username; } set { username = value; } }
        public string InvitationUsername { get { return invitationUsername; } set { invitationUsername = value; } }
        public List<Friend> FriendsList { get { return friendsList; } set { friendsList = value; } }

        public ChatModel(ServerConnection connection) : base(connection) {}

        public bool CheckUsernameText(string username) {
            if (!String.IsNullOrEmpty(username) && Regex.Match(username, @"^[\w]{3,}$").Success) return true;
            else return false;
        }

        public string GetFriendsJSON() {
            ServerCommands.GetFriendsCommandResponse response = ServerCommands.GetFriendsCommand(ref connection);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return response.friendsJSON;
        }

        public void LogoutUser() {
            int error = ServerCommands.LogoutCommand(ref connection);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public bool CheckUserExist(string username) {
            int error = ServerCommands.CheckUsernameExistCommand(ref connection, username);
            if (error == (int)ErrorCodes.NO_ERROR) return false;
            else if (error == (int)ErrorCodes.USER_ALREADY_EXISTS) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

    }
}
