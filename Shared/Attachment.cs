using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public enum FILE_STATUS
    {
        NOT_SENT,
        SENDING,
        SENT,
        ERROR
    }
    public class Attachment
    {
        public string Name { get; set; }
        public FILE_STATUS fStatus { get; set; }

        public UInt32 attachmentID { get; set; }

        public Attachment(string name)
        {
            fStatus = FILE_STATUS.NOT_SENT;
            Name = name;
        }
    }
}
