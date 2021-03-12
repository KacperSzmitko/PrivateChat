using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client.Models
{
    public class RegisterModel : BaseModel
    {
        public bool CheckUsernameText(string username) {
            if (Regex.Match(username, @"^[\w]{3,}$").Success) return true;
            else return false;
        }

        public bool CheckUsernameExist(string username) {
            int error = ServerCommands.CheckUsernameExistCommand(ref connection, username);
            if (error == (int)ErrorCodes.NO_ERROR) return false;
            else if (error == (int)ErrorCodes.USER_ALREADY_EXISTS) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public bool CheckPasswordText(string pass) {
            if (pass.Length >= 8) return true;
            else return false;
        }

        public bool CheckPasswordsAreEqual(string pass1, string pass2) {
            if (pass1 == pass2) return true;
            else return false;
        }

        public bool RegisterUser(string username, string pass) {
            int error = ServerCommands.RegisterUser(ref connection, username, pass);
            if (error == (int)ErrorCodes.NO_ERROR) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public RegisterModel(ServerConnection connection) : base(connection) {
            
        }
    }
}
