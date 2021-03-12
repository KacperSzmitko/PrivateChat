using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    class MainModel : BaseModel
    {
        public MainModel(ServerConnection connection) : base(connection) { }

        public void Disconnect() {
            int error = ServerCommands.DisconnectCommand(ref connection);
            if (error != (int)ErrorCodes.NO_ERROR && error != (int)ErrorCodes.NOT_LOGGED_IN) throw new Exception(GetErrorCodeName(error));
        }
    }
}
