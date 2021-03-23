using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Client.Models
{
    public class ChatModel : BaseModel
    {
        private readonly string username;
        private readonly string userPath;
        private readonly string encryptedUserKeyFilePath;
        private readonly byte[] userKey;

        private string invitationUsername;
        private List<FriendStatus> friends;
        private List<Invitation> receivedInvitations;

        public byte[] UserKey { get { return userKey; } }
        public string Username { get { return username; } }
        public string InvitationUsername { get { return invitationUsername; } set { invitationUsername = value; } }
        public List<FriendStatus> Friends { get { return friends; } set { friends = value; } }
        public List<Invitation> ReceivedInvitations { get { return receivedInvitations; } set { receivedInvitations = value; } }

        public ChatModel(ServerConnection connection, string username, byte[] userKey) : base(connection) {
            this.username = username;
            this.userKey = userKey;
            this.userPath = Path.Combine(appLocalDataFolderPath, username);
            this.encryptedUserKeyFilePath = Path.Combine(userPath, encryptedUserKeyFileName);
            this.friends = new List<FriendStatus>();
            this.receivedInvitations = new List<Invitation>();
            Directory.CreateDirectory(userPath);
        }

        public bool CheckUsernameText(string username) {
            if (!String.IsNullOrEmpty(username) && Regex.Match(username, @"^[\w]{3,}$").Success) return true;
            else return false;
        }

        public void LogoutUser() {
            int error = ServerCommands.LogoutCommand(ref connection);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public bool CheckUserExist(string username) {
            int error = ServerCommands.CheckUsernameExistCommand(ref connection, username);
            if (error == (int)ErrorCodes.NO_ERROR) return false;
            else if (error == (int)ErrorCodes.USER_ALREADY_EXISTS) return true;
            else throw new Exception(GetErrorCodeName(error));
        }

        public (string g, string p, string invitationID) SendInvitation(string username) {
            var response = ServerCommands.SendInvitationCommand(ref connection, username);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            return (response.g, response.p, response.invitationID);
        }

        public (string publicDHKey, byte[] privateDHKey) GenerateDHKeys(string p, string g) {
            var parameters = new DHParameters(new Org.BouncyCastle.Math.BigInteger(p, 16), new Org.BouncyCastle.Math.BigInteger(g, 16));
            var DHkeys = Security.GenerateKeys(parameters);
            return (Security.GetPublicKey(DHkeys), Security.GetPrivateKeyBytes(DHkeys));
        }

        public byte[] GenerateConversationKey(string publicKeyA, string p, string g, byte[] privateKeyB) {
            return Security.ComputeSharedSecret(publicKeyA, Security.ByteArrayToHexString(privateKeyB), p, g).ToByteArray();
        }

        public void SendPublicDHKey(string invitationID, string publicDHKey, string privateEncryptedDHKey, string privateEncryptedDHKeyIV) {
            int error = ServerCommands.SendPublicDHKeyCommand(ref connection, invitationID, publicDHKey, privateEncryptedDHKey, privateEncryptedDHKeyIV);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public void GetFriends() {
            var response = ServerCommands.GetFriendsCommand(ref connection);
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            List<Friend> friendsNoUnreadMessage = JsonConvert.DeserializeObject<List<Friend>>(response.friendsJSON);
            foreach (Friend friendNoUnreadMessage in friendsNoUnreadMessage) {
                bool newFriend = true;
                for (int i = 0; i < friends.Count; i++) {
                    if (friendNoUnreadMessage.username == friends[i].Name) {
                        newFriend = false;
                        friends[i].Active = Convert.ToBoolean(friendNoUnreadMessage.active);
                    }
                }
                if (newFriend) friends.Add(new FriendStatus(friendNoUnreadMessage.username, Convert.ToBoolean(friendNoUnreadMessage.active)));
            }
        }

        public void GetNotifications() {
            var response = ServerCommands.GetNotificationsCommand(ref connection);
            if (response.error == (int)ErrorCodes.NO_NOTIFICATIONS) return;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            List<Notification> notificationsList = JsonConvert.DeserializeObject<List<Notification>>(response.newMessagesInfoJSON);
            foreach (Notification notification in notificationsList) {
                for (int i = 0; i < friends.Count; i++) {
                    if (notification.username == friends[i].Name) friends[i].NotificationsAmount = notification.numberOfMessages;
                }
            }
        }

        public void GetInvitations() {
            var response = ServerCommands.GetInvitationsCommand(ref connection);
            if (response.error == (int)ErrorCodes.NOTHING_TO_SEND) return;
            if (response.error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(response.error));
            receivedInvitations = JsonConvert.DeserializeObject<List<Invitation>>(response.invitationsJSON);
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
    }
}
