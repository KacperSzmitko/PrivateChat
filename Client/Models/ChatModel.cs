using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public class ChatModel : BaseModel
    {
        public ChatModel(ServerConnection connection) : base(connection) {}

        public string GetFriendsJSON() {
            ServerCommands.GetFriendsCommandResponse response = ServerCommands.GetFriendsCommand(ref connection);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return response.friendsJSON;
        }

        public void LogoutUser() {
            int error = ServerCommands.LogoutCommand(ref connection);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

    }
}
