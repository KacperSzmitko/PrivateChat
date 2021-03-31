using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Client.Models
{
    public class ChatModel : BaseModel
    {
        private readonly string username;
        private readonly string userPath;
        private readonly string encryptedUserKeyFilePath;
        private readonly byte[] userKey;

        private List<FriendItem> friends;
        private List<Invitation> receivedInvitations;
        private Dictionary<string, Conversation> conversations;

        public byte[] UserKey { get { return userKey; } }
        public string Username { get { return username; } }
        public List<FriendItem> Friends { get { return friends; } set { friends = value; } }
        public List<Invitation> ReceivedInvitations { get { return receivedInvitations; } set { receivedInvitations = value; } }
        public Dictionary<string, Conversation> Conversations { get { return conversations; } set { conversations = value; } }

        public ChatModel(ServerConnection connection, string username, byte[] userKey) : base(connection) {
            this.username = username;
            this.userKey = userKey;
            this.userPath = Path.Combine(appLocalDataFolderPath, username);
            this.encryptedUserKeyFilePath = Path.Combine(userPath, encryptedUserKeyFileName);
            this.friends = new List<FriendItem>();
            this.receivedInvitations = new List<Invitation>();
            this.conversations = new Dictionary<string, Conversation>();
            Directory.CreateDirectory(userPath);
        }

        public bool CheckUsernameText(string username) {
            if (!String.IsNullOrEmpty(username) && Regex.Match(username, @"^[\w]{3,}$").Success) return true;
            else return false;
        }

        public void Logout() {
            int error = ServerCommands.LogoutCommand(ref connection);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public bool CheckUserExist(string username) {
            int error = ServerCommands.CheckUsernameExistCommand(ref connection, username);
            if (error == (int)ErrorCodes.NO_ERROR) return false;
            else if (error == (int)ErrorCodes.USER_ALREADY_EXISTS) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public (InvitationStatuses invitationStatus, string g, string p, string invitationID) SendInvitation(string username) {
            var response = ServerCommands.SendInvitationCommand(ref connection, username);
            if (response.error == (int)ErrorCodes.SELF_INVITE_ERROR) return (InvitationStatuses.SELF_INVITATION, "", "", "");
            if (response.error == (int)ErrorCodes.INVITATION_ALREADY_EXIST) return (InvitationStatuses.INVITATION_ALREADY_EXIST, "", "", "");
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return (InvitationStatuses.INVITATION_SENT, response.g, response.p, response.invitationID);
        }

        public (string publicDHKey, byte[] privateDHKey) GenerateDHKeys(string p, string g) {
            var parameters = new DHParameters(new Org.BouncyCastle.Math.BigInteger(p, 16), new Org.BouncyCastle.Math.BigInteger(g, 16));
            var DHkeys = Security.GenerateKeys(parameters);
            return (Security.GetPublicKey(DHkeys), Security.GetPrivateKeyBytes(DHkeys));
        }

        public byte[] GenerateConversationKey(string publicKeyA, string p, string g, byte[] privateKeyB) {
            return Security.ComputeSharedSecret(publicKeyA, Security.ByteArrayToHexString(privateKeyB), p, g);
        }

        public void SendPublicDHKey(string invitationID, string publicDHKey, string privateEncryptedDHKey, string privateEncryptedDHKeyIV) {
            int error = ServerCommands.SendPublicDHKeyCommand(ref connection, invitationID, publicDHKey, privateEncryptedDHKey, privateEncryptedDHKeyIV);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public void GetFriends() {
            var response = ServerCommands.GetFriendsCommand(ref connection);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            List<Friend> dirtyFriends = JsonConvert.DeserializeObject<List<Friend>>(response.friendsJSON);
            foreach (Friend dirtyFriend in dirtyFriends) {
                bool newFriend = true;
                for (int i = 0; i < friends.Count; i++) {
                    if (dirtyFriend.username == friends[i].Name) {
                        newFriend = false;
                        friends[i].Active = Convert.ToBoolean(dirtyFriend.active);
                    }
                }
                if (newFriend) friends.Add(new FriendItem(dirtyFriend.username, Convert.ToBoolean(dirtyFriend.active)));
            }
        }

        public bool GetNotifications() {
            var response = ServerCommands.GetNotificationsCommand(ref connection);
            if (response.error == (int)ErrorCodes.NO_NOTIFICATIONS) return false;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            List<Notification> notificationsList = JsonConvert.DeserializeObject<List<Notification>>(response.newMessagesInfoJSON);
            foreach (Notification notification in notificationsList) {
                for (int i = 0; i < friends.Count; i++) {
                    if (notification.username == friends[i].Name) friends[i].NotificationsAmount += notification.numberOfMessages;
                }
            }
            return true;
        }

        public bool GetInvitations() {
            var response = ServerCommands.GetInvitationsCommand(ref connection);
            if (response.error == (int)ErrorCodes.NOTHING_TO_SEND) return false;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            receivedInvitations = JsonConvert.DeserializeObject<List<Invitation>>(response.invitationsJSON);
            return true;
        }

        public (string conversationID, byte[] conversationIV) AcceptFriendInvitation(string invitationID, string PKB) {
            var response = ServerCommands.AcceptFriendInvitationCommand(ref connection, invitationID, PKB);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return (response.conversationID, Security.HexStringToByteArray(response.conversationIV));
        }

        public void DeclineFriendInvitation(string invitationID) {
            var error = ServerCommands.DeclineFriendInvitationCommand(ref connection, invitationID);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public void SendEncryptedConversationKey(string conversationID, byte[] encryptedConversationKey) {
            string encryptedConversationKeyHexString = Security.ByteArrayToHexString(encryptedConversationKey);
            int error = ServerCommands.SendEncryptedConversationKeyCommand(ref connection, conversationID, encryptedConversationKeyHexString);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public List<ExtendedInvitation> GetAcceptedInvitations() {
            var response = ServerCommands.GetAcceptedInvitationsCommand(ref connection);
            if (response.error == (int)ErrorCodes.NOTHING_TO_SEND) return null;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return JsonConvert.DeserializeObject<List<ExtendedInvitation>>(response.ExtendedInvitationJSON);
        }

        public List<MessageItem> DecryptMessages(List<Message> dirtyMessages, byte[] conversationKey) {
            List<MessageItem> messages = new List<MessageItem>();
            if (dirtyMessages.Count > 0) {
                foreach (Message dirtyMessage in dirtyMessages) {
                    byte[] encryptedMessageTextBytes = Security.HexStringToByteArray(dirtyMessage.message);
                    byte[] messageTextIV = Security.HexStringToByteArray(dirtyMessage.iv);
                    byte[] messageTextBytes = Security.AESDecrypt(encryptedMessageTextBytes, conversationKey, messageTextIV);
                    string messageText = Encoding.Unicode.GetString(messageTextBytes);
                    messages.Add(new MessageItem(dirtyMessage.username, messageText, dirtyMessage.date, dirtyMessage.username == username));
                }
            }
            return messages;
        }

        public string CreateEncryptedMessageToSendJSON(string friendUsername, string messageToSendText) {
            byte[] messageTextBytes = Encoding.Unicode.GetBytes(messageToSendText);
            byte[] iv = Security.GenerateIV();
            byte[] encryptedMessageTextBytes = Security.AESEncrypt(messageTextBytes, conversations[friendUsername].ConversationKey, iv);
            string ivHexString = Security.ByteArrayToHexString(iv);
            string encryptedMessageTextHexString = Security.ByteArrayToHexString(encryptedMessageTextBytes);
            Message messageToSend = new Message();
            messageToSend.message = encryptedMessageTextHexString;
            messageToSend.date = DateTime.MinValue;
            messageToSend.iv = ivHexString;
            messageToSend.username = username;
            return JsonConvert.SerializeObject(messageToSend);
        }

        public int AddUserMessageToConversation(string friendUsername, string messageToSendText) {
            conversations[friendUsername].Messages.Add(new MessageItem(username, messageToSendText, DateTime.Now, true));
            return conversations[friendUsername].Messages.Count - 1;
        }

        public void GetConversation(string friendUsername) {
            var response = ServerCommands.GetConversationCommand(ref connection, friendUsername);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            byte[] encryptedConversationKey = Security.HexStringToByteArray(response.conversationKey);
            byte[] conversationIV = Security.HexStringToByteArray(response.conversationIV);
            byte[] conversationKey = Security.AESDecrypt(encryptedConversationKey, userKey, conversationIV);
            List<Message> dirtyMessages = new List<Message>();
            if (response.messagesJSON != null && response.messagesJSON != "" && response.messagesJSON != "[{}]") dirtyMessages = JsonConvert.DeserializeObject<List<Message>>(response.messagesJSON);
            conversations[friendUsername] = new Conversation(response.conversationID, conversationKey, conversationIV, DecryptMessages(dirtyMessages, conversationKey));
        }

        public void ActivateConversation(string friendUsername) {
            int error = ServerCommands.ActivateConversationCommand(ref connection, conversations[friendUsername].ConversationID);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public bool GetMessages(string friendUsername) {
            var response = ServerCommands.GetNewMessagesCommand(ref connection);
            if (response.error == (int)ErrorCodes.NO_MESSAGES) return false;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            List<Message> dirtyMessages = JsonConvert.DeserializeObject<List<Message>>(response.newMessegesJSON);
            byte[] conversationKey = conversations[friendUsername].ConversationKey;
            conversations[friendUsername].Messages.AddRange(DecryptMessages(dirtyMessages, conversationKey));
            return true;
        }

        public void SendMessage(string friendUsername, string messageJSON) {
            int error = ServerCommands.SendMessageCommand(ref connection, conversations[friendUsername].ConversationID, messageJSON);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public void DeleteAccount() {
            int error = ServerCommands.DeleteAccountCommand(ref connection);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public void RemoveNotification(string friendUsername) {
            for (int i = 0; i < friends.Count; i++) {
                if (friends[i].Name == friendUsername) friends[i].NotificationsAmount = 0;
            }
        }
    }
}
