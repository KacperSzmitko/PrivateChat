using Shared;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Client.Models
{
    public class AttachmentModel : BaseModel
    {
        private string username;

        public string Username { get { return username; } set { username = value; } }

        public AttachmentModel(ServerConnection connection) : base(connection) { }

    }
}
