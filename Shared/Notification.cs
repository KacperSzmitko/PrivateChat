using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    [Serializable]
    public class Notification
    {
        public string username { get; set; }
        public int numberOfMessages { get; set; }
    }
}
