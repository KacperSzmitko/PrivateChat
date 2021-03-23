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
        public string g;
        public string p;
        [NonSerialized] public int invitationId;
        public string publicKeyReciver;
        [NonSerialized] public bool accepted = false;
        public string conversationId;
        public string conversationIv;
        public string encryptedSenderPrivateKey;
        public string ivToDecryptSenderPrivateKey;

        public static implicit operator Invitation(ExtendedInvitation e) => new Invitation { g = e.g, p = e.p, invitationId = e.invitationId, publicKeySender = e.publicKeySender,sender = e.sender };
    }
}
