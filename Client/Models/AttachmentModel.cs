using Shared;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Client.Models
{
    public class AttachmentModel : BaseModel
    {
        private string username;
        private List<Attachment> attachments;

        public List<Attachment> Attachments { get { return attachments; } set { attachments = value; } }
        public string Username { get { return username; } set { username = value; } }

        public AttachmentModel(ServerConnection connection) : base(connection) {
            this.attachments = new List<Attachment> { new Attachment("test1"), new Attachment("test2"), new Attachment("test3") };
        }

    }
}
