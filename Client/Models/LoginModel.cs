using Shared;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Client.Models
{
    public class LoginModel : BaseModel
    {
        private string username;
        private string pass;

        public string Username { get { return username; } set { username = value; } }
        public string Pass { get { return pass; } set { pass = value; } }

        public LoginModel(ServerConnection connection) : base(connection) {}
        
        public string LoginUser(string username, string password) {
            int response = ServerCommands.LoginCommand(ref connection, username, password);
            if (response == (int)ErrorCodes.NO_ERROR) return username;
            else if (response == (int)ErrorCodes.USER_NOT_FOUND || response == (int)ErrorCodes.INCORRECT_PASSWORD) return null;
            else throw new Exception(GetErrorCodeName(response)); 
        }

        public byte[] CreateCredentialsHash(string username, string password, string bonus) {
            return SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(username + "$$" + password + "$$" + bonus + "$$"));
        }
    }
}
