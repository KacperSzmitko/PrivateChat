using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public class LoginModel : BaseModel
    {
        public string LoginUser(string username, string password) {
            int response = ServerCommands.LoginCommand(ref connection, username, password);
            if (response == (int)ErrorCodes.NO_ERROR) return username;
            else if (response == (int)ErrorCodes.USER_NOT_FOUND || response == (int)ErrorCodes.INCORRECT_PASSWORD) return null;
            else throw new Exception(GetErrorCodeName(response)); 
        }

        public LoginModel(ServerConnection connection) : base(connection) {}
    }
}
