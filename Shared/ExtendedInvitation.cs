using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    [Serializable]
    public class ExtendedInvitation
    {
        [NonSerialized] public string sender;
        [NonSerialized] public string reciver;
        [NonSerialized] public bool sended  = false;
        [NonSerialized] public string publicKeySender;
        [NonSerialized] public string g;
        [NonSerialized] public string p;
        public int invitationId;
        public string publicKeyReciver;
        [NonSerialized] public bool accepted = false;
        public string conversationId;
        public string conversationIv;

        public static implicit operator Invitation(ExtendedInvitation e) => new Invitation { g = e.g, p = e.p, invitationId = e.invitationId, publicKeySender = e.publicKeySender,conversationIv = e.conversationIv };
    }
}
