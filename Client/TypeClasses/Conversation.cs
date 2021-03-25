using System.Collections.Generic;

namespace Client
{
    public class Conversation
    {
        public string ConversationID { get; set; }
        public byte[] ConversationKey { get; set; }
        public byte[] ConversationIV { get; set; }
        public List<MessageItem> Messages { get; set; }

        public Conversation(string conversationID, byte[] conversationKey, byte[] conversationIV, List<MessageItem> messages) {
            ConversationID = conversationID;
            ConversationKey = conversationKey;
            ConversationIV = conversationIV;
            Messages = messages;
        }
    }
}
