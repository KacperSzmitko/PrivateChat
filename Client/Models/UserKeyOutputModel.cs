using System;
using System.Text;

namespace Client.Models
{
    public class UserKeyOutputModel : BaseModel
    {
        private readonly string userKeyHexString;

        public string UserKeyHexString { get { return userKeyHexString; } }

        public UserKeyOutputModel(ServerConnection connection, string userKeyHexString) : base(connection) {
            this.userKeyHexString = userKeyHexString;
        }
    }
}
