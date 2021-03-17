using Shared;
using System;
using System.IO;

namespace Client.Models
{
    public abstract class BaseModel
    {
        protected ServerConnection connection;

        protected const string appLocalDataFolderName = "private_chat";
        protected const string invitationKeysFileName = "invitation_keys.json";
        protected const string encryptedUserKeyFileName = "encrypted_user_key.key";

        protected readonly string appLocalDataFolderPath;

        public BaseModel(ServerConnection connection) {
            this.connection = connection;
            this.appLocalDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appLocalDataFolderName);
        }

        protected string GetErrorCodeName(int errorCode) { return Enum.GetName(typeof(ErrorCodes), errorCode); }
    }
}
