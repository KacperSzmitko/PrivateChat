using Shared;
using System;
using System.IO;
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
        
        public (byte[] userIV, byte[] userKeyHash) LoginUser() {
            (int error, string userIV, string userKeyHash) = ServerCommands.LoginCommand(ref connection, username, pass);
            if (error == (int)ErrorCodes.NO_ERROR) return (Security.HexStringToByteArray(userIV), Security.HexStringToByteArray(userKeyHash));
            else if (error == (int)ErrorCodes.USER_NOT_FOUND || error == (int)ErrorCodes.INCORRECT_PASSWORD) return (null, null);
            else throw new Exception(GetErrorCodeName(error)); 
        }

        public byte[] CreateCredentialsHash(byte[] userIV) {
            return Security.CreateSHA256Hash(Encoding.ASCII.GetBytes(username + "$$" + pass + "$$" + Security.ByteArrayToHexString(userIV)));
        }

        public byte[] VerifyAndGetUserKey(byte[] decryptingKey, byte[] userIV, byte[] userKeyHash) {
            string userPath = Path.Combine(appLocalDataFolderPath, username);
            string encryptedUserKeyFilePath = Path.Combine(userPath, encryptedUserKeyFileName);

            //If encryptedUserKey dosen't exist - return null
            if (!File.Exists(encryptedUserKeyFilePath)) return null;
            string encryptedUserKeyHexString = File.ReadAllText(encryptedUserKeyFilePath);

            //If encryptedUserKey length is wrong - return null
            if (encryptedUserKeyHexString.Length != 64) return null;
            byte[] encryptedUserKey = Security.HexStringToByteArray(encryptedUserKeyHexString);
            byte[] userKey = Security.AESDecrypt(encryptedUserKey, decryptingKey, userIV);

            //If userKey hash dosen't match hash from database - return null
            if (Security.CreateSHA256Hash(userKey) != userKeyHash) return null;

            //If everything is ok - return userKey
            return userKey;
        }
    }
}
