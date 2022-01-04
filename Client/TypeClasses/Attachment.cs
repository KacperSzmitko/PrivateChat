using System;
using System.Collections.Generic;
using System.Text;

namespace Client
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

        public Attachment(string name)
        {
            fStatus = FILE_STATUS.NOT_SENT;
            Name = name;
        }
    }
}
