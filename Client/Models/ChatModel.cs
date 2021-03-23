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
        private readonly string invitationKeysFilePath;
        private readonly string encryptedUserKeyFilePath;
        private readonly byte[] userKey;
        private readonly byte[] userIV;
        private readonly byte[] credentialsHash;

        private string invitationUsername;
        private List<FriendStatus> friends;
        private List<Invitation> receivedInvitations;

        private readonly object DHKeysFileOperationsLock;

        public byte[] CredentialsHash { get { return credentialsHash; } }
        public byte[] UserKey { get { return userKey; } }
        public byte[] UserIV { get { return userIV; } }
        public string Username { get { return username; } }
        public string InvitationUsername { get { return invitationUsername; } set { invitationUsername = value; } }
        public List<FriendStatus> Friends { get { return friends; } set { friends = value; } }
        public List<Invitation> ReceivedInvitations { get { return receivedInvitations; } set { receivedInvitations = value; } }

        public ChatModel(ServerConnection connection, string username, byte[] userKey, byte[] userIV, byte[] credentialsHash) : base(connection) {
            this.username = username;
            this.userKey = userKey;
            this.userIV = userIV;
            this.credentialsHash = credentialsHash;
            this.userPath = Path.Combine(appLocalDataFolderPath, username);
            this.invitationKeysFilePath = Path.Combine(userPath, invitationKeysFileName);
            this.encryptedUserKeyFilePath = Path.Combine(userPath, encryptedUserKeyFileName);
            this.friends = new List<FriendStatus>();
            this.receivedInvitations = new List<Invitation>();
            this.DHKeysFileOperationsLock = new object();
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

        public void SendPublicDHKey(string invitationID, string publicDHKey) {
            int error = ServerCommands.SendPublicDHKeyCommand(ref connection, invitationID, publicDHKey);
            if (error != (int)ErrorCodes.NO_ERROR) throw new Exception(GetErrorCodeName(error));
        }

        public (string publicDHKey, byte[] privateDHKey) GenerateDHKeys(string g, string p) {
            var parameters = new DHParameters(new Org.BouncyCastle.Math.BigInteger(p, 16), new Org.BouncyCastle.Math.BigInteger(g, 16));
            var DHkeys = Security.GenerateKeys(parameters);
            return (Security.GetPublicKey(DHkeys), Security.GetPrivateKeyBytes(DHkeys));
        }

        public void SaveEncryptedPrivateDHKey(string invitationID, byte[] encryptedPrivateDHKey) {
            lock (DHKeysFileOperationsLock) {
                Dictionary<string, string> invitationKeys;
                if (!File.Exists(invitationKeysFilePath)) invitationKeys = new Dictionary<string, string>();
                else {
                    string invitationKeysFileContent = File.ReadAllText(invitationKeysFilePath);
                    invitationKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(invitationKeysFileContent);
                }
                if (invitationKeys.ContainsKey(invitationID)) invitationKeys[invitationID] = Security.ByteArrayToHexString(encryptedPrivateDHKey);
                else invitationKeys.Add(invitationID, Security.ByteArrayToHexString(encryptedPrivateDHKey));
                string invitationKeysFileContentToSave = JsonConvert.SerializeObject(invitationKeys);
                File.WriteAllText(invitationKeysFilePath, invitationKeysFileContentToSave);
            }
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

        public (byte[] conversationKey, string publicDHKey) GenerateConversationKeyAndPublicDHKey(string invitingPublicDHKey, string p, string g) {
            var parameters = new DHParameters(new Org.BouncyCastle.Math.BigInteger(p, 16), new Org.BouncyCastle.Math.BigInteger(g, 16));
            var DHKeys = Security.GenerateKeys(parameters);
            var conversationKeyInt = Security.ComputeSharedSecret(invitingPublicDHKey, DHKeys.Private, parameters);
            return (conversationKeyInt.ToByteArray(), Security.GetPublicKey(DHKeys));
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

        public byte[] GetEncryptedPrivateDHKey(string invitationID) {
            lock (DHKeysFileOperationsLock) {
                Dictionary<string, string> invitationKeys;
                if (!File.Exists(invitationKeysFilePath)) return null;
                string invitationKeysFileContent = File.ReadAllText(invitationKeysFilePath);
                invitationKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(invitationKeysFileContent);
                if (invitationKeys.ContainsKey(invitationID)) return Security.HexStringToByteArray(invitationKeys[invitationID]);
                else return null;
            }
        }

        public byte[] GenerateConversationKey(string invitedPublicDHKey, string p, string g, byte[] privateDHKey) {
            var parameters = new DHParameters(new Org.BouncyCastle.Math.BigInteger(p, 16), new Org.BouncyCastle.Math.BigInteger(g, 16));
            var test = new DHPrivateKeyParameters(new Org.BouncyCastle.Math.BigInteger(Security.ByteArrayToHexString(privateDHKey), 16), parameters);
            var conversationKeyInt = Security.ComputeSharedSecret(invitedPublicDHKey, test, parameters);
        }
    }
}
