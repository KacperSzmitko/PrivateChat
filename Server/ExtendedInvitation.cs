using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class ExtendedInvitation
    {
        public string sender { get; set; }
        public string reciver { get; set; }
        public bool sended { get; set; } = false;
        public string publicKeySender { get; set; }
        public string g { get; set; }
        public string p { get; set; }
        public int invitationId { get; set; }
        public string publicKeyReciver { get; set; }
        public bool accepted { get; set; } = false;
        public string conversationId { get; set; }

        public static implicit operator Invitation(ExtendedInvitation e) => new Invitation { g = e.g, p = e.p, invitationId = e.invitationId, publicKeySender = e.publicKeySender };

    }
}
