using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public class LoginModel : BaseModel
    {
        public User LoginUser(string username, string password) {
            ServerCommands.LoginCommandResponse response = ServerCommands.LoginCommand(ref connection, username, password);
            if (response.error == (int)ErrorCodes.NO_ERROR) return new User(response.sessionID, username, response.elo);
            else if (response.error == (int)ErrorCodes.USER_NOT_FOUND || response.error == (int)ErrorCodes.INCORRECT_PASSWORD) return null;
            else throw new Exception(GetErrorCodeName(response.error)); 
        }

        public LoginModel(ServerConnection connection) : base(connection) {

        }
    }
}
