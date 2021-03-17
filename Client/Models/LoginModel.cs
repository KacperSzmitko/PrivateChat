using Shared;
using System;
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
        
        public byte[] LoginUser(string username, string password) {
            (int error, string userIV) = ServerCommands.LoginCommand(ref connection, username, password);
            if (error == (int)ErrorCodes.NO_ERROR) return Security.HexStringToByteArray(userIV);
            else if (error == (int)ErrorCodes.USER_NOT_FOUND || error == (int)ErrorCodes.INCORRECT_PASSWORD) return null;
            else throw new Exception(GetErrorCodeName(error)); 
        }

        public byte[] CreateCredentialsHash(string username, string password, byte[] userIV) {
            return Security.CreateSHA256Hash(Encoding.ASCII.GetBytes(username + "$$" + password + "$$" + Security.ByteArrayToHexString(userIV)));
        }
    }
}
