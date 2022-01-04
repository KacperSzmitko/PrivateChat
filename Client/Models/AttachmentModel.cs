using Shared;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Client.ViewModels;

namespace Client.Models
{
    public class AttachmentModel : BaseModel
    {
        private string username;
        private List<Attachment> attachments;
        private Boolean lockNewAttachments = true;

        public List<Attachment> Attachments { get { return attachments; } set { attachments = value; } }
        public string Username { get { return username; } set { username = value; } }

        public AttachmentModel(ServerConnection connection) : base(connection) {
            this.attachments = new List<Attachment> { };
        }

        public bool GetNewAttachments(string conversationID)
        {
            if (lockNewAttachments)
                return false;
            var response = ServerCommands.GetNewAttachmentsCommand(ref connection, conversationID);
            if (response.error == (int)ErrorCodes.NO_MESSAGES) return false;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));

            List<Attachment> attachments = JsonConvert.DeserializeObject<List<Attachment>>(response.newMessegesJSON);
            if (attachments.Count < 1)
                return false;
            this.attachments.AddRange(attachments);

            return true;
        }

        public bool GetAttachments(string conversationID, AttachmentViewModel avm)
        {
            var response = ServerCommands.GetAttachmentsCommand(ref connection, conversationID);
            if (response.error == (int)ErrorCodes.NO_MESSAGES) return false;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));

            List<Attachment> attachments = JsonConvert.DeserializeObject<List<Attachment>>(response.newMessegesJSON);
            if (attachments.Count < 1)
            {
                lockNewAttachments = false;
                return false;
            }
                
            this.attachments = attachments;
            avm.OnPropertyChangedUp("Attachments");
            lockNewAttachments = false;
            return true;
        }

    }
}
