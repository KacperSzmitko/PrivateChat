using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Models
{
    public abstract class BaseModel
    {
        protected ServerConnection connection;

        public BaseModel(ServerConnection connection) {
            this.connection = connection;
        }

        protected string GetErrorCodeName(int errorCode) { return Enum.GetName(typeof(ErrorCodes), errorCode); }
    }
}
