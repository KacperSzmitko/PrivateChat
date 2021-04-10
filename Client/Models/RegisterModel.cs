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

        public bool CheckUsernameText() {
            if (Regex.Match(username, "^[\\w]{3,}$").Success) return true;
            else return false;
        }

        public bool CheckUsernameExist() {
            int error = ServerCommands.CheckUsernameExistCommand(ref connection, username);
            if (error == (int)ErrorCodes.NO_ERROR) return false;
            else if (error == (int)ErrorCodes.USER_ALREADY_EXISTS) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public bool CheckPasswordText() {
            if (Regex.Match(pass1, "(?=.*[!\"#%&'()*+,\\-\\./:<>=?@\\[\\]\\^_{}|~])(?=.*[A-Z])(?!.*\\$).{8,255}").Success) return true;
            else return false;
        }

        public bool CheckPasswordsAreEqual() {
            if (pass1 == pass2) return true;
            else return false;
        }

        public bool RegisterUser(byte[] userIV, byte[] userKeyHash) {
            int error = ServerCommands.RegisterUser(ref connection, username, pass2, Security.ByteArrayToHexString(userIV), Security.ByteArrayToHexString(userKeyHash));
            if (error == (int)ErrorCodes.NO_ERROR) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public byte[] CreateCredentialsHash(byte[] userIV) {
            return Security.CreateSHA256Hash(Encoding.ASCII.GetBytes(username + "$$" + pass2 + "$$" + Security.ByteArrayToHexString(userIV)));
        }

        public void SaveEncryptedUserKey(byte[] userKey, byte[] encryptingKey, byte[] userIV) {
            //Przypisz do zmiennej ścieżke do folderu użytkownika
            string userPath = Path.Combine(appLocalDataFolderPath, username);
            //Przypisz do zmiennej ścieżke do pliku w którym ma być umieszczony zaszyfrowany klucz użytkownika
            string encryptedUserKeyFilePath = Path.Combine(userPath, encryptedUserKeyFileName);
            //Zaszyfruj klucz użytkownika
            byte[] encryptedUserKey = Security.AESEncrypt(userKey, encryptingKey, userIV);
            //Zamień zaszyfrowane bajty klucza użytkownika na tekstowy zapis szesnastkowy
            string encryptedUserKeyHexString = Security.ByteArrayToHexString(encryptedUserKey);
            //Jeśli folder użytkownika nie istnieje, utwórz go
            Directory.CreateDirectory(userPath);
            //Zapisz zawartość zmiennej "encryptedUserKeyHexString" do pliku o ścieżce "encryptedUserKeyFilePath"
            File.WriteAllText(encryptedUserKeyFilePath, encryptedUserKeyHexString);
        }
    }
}
