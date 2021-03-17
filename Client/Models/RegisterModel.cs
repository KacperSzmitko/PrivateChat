using Shared;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Client.Models
{
    public class RegisterModel : BaseModel
    {
        private string username;
        private string pass1;
        private string pass2;

        public string Username { get { return username; } set { username = value; } }
        public string Pass1 { get { return pass1; } set { pass1 = value; } }
        public string Pass2 { get { return pass2; } set { pass2 = value; } }

        public RegisterModel(ServerConnection connection) : base(connection) { }

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

        public bool RegisterUser(string username, string pass, byte[] userIV, byte[] userKeyHash) {
            int error = ServerCommands.RegisterUser(ref connection, username, pass, Security.ByteArrayToHexString(userIV), Security.ByteArrayToHexString(userKeyHash));
            if (error == (int)ErrorCodes.NO_ERROR) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public byte[] CreateCredentialsHash(string username, string password, byte[] userIV) {
            return Security.CreateSHA256Hash(Encoding.ASCII.GetBytes(username + "$$" + password + "$$" + Security.ByteArrayToHexString(userIV)));
        }

        public void SaveEncryptedUserKey(byte[] userKey, byte[] encryptingKey, byte[] userIV) {
            string userPath = Path.Combine(appLocalDataFolderPath, username);
            string encryptedUserKeyFilePath = Path.Combine(userPath, encryptedUserKeyFileName);
            byte[] encryptedUserKey = Security.AESEncrypt(userKey, encryptingKey, userIV);
            string encryptedUserKeyHexString = Security.ByteArrayToHexString(encryptedUserKey);
            Directory.CreateDirectory(userPath);
            File.WriteAllText(encryptedUserKeyFilePath, encryptedUserKeyHexString);
        }
    }
}
