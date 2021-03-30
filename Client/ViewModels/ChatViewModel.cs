using Client.Commands;
using Client.Models;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private const int baseRefreshRate = 200;

        private ChatModel model;

        private Thread sendInvitationThread;
        private Thread acceptFriendThread;
        private Thread declineFriendThread;
        private Thread loadConversationThread;
        private Thread sendMessageThread;
        private Thread updateThread;

        private RelayCommand sendInvitationCommand;
        private RelayCommand acceptInvitationCommand;
        private RelayCommand declineInvitationCommand;
        private RelayCommand sendMessageCommand;
        private RelayCommand logoutCommand;

        private InvitationStatus lastInvitationStatus;
        private Invitation lastRecivedInvitation;
        private FriendItem selectedFriend;

        private string invitationUsername;
        private string messageToSendText;
        private bool activeConversation;
        private bool keepUpdating;

        public string Username { get { return model.Username; } }

        public ObservableCollection<FriendItem> Friends { 
            get { 
                return new ObservableCollection<FriendItem>(model.Friends); 
            } 
        }

        public ObservableCollection<MessageItem> Messages { 
            get {
                if (selectedFriend != null) return new ObservableCollection<MessageItem>(model.Conversations[selectedFriend.Name].Messages);
                else return null;
            } 
        }

        public string UserNotFoundErrorVisibility {
            get {
                if (lastInvitationStatus == InvitationStatus.USER_NOT_FOUND) return "Visible";
                else return "Collapsed";
            }
        }

        public string UserAlreadyAFriendErrorVisibility {
            get {
                if (lastInvitationStatus == InvitationStatus.USER_ALREADY_A_FRIEND) return "Visible";
                else return "Collapsed";
            }
        }

        public string InvitationAlredyExistErrorVisibility {
            get {
                if (lastInvitationStatus == InvitationStatus.INVITATION_ALREADY_EXIST) return "Visible";
                else return "Collapsed";
            }
        }

        public string SelfInvitationErrorVisibility {
            get {
                if (lastInvitationStatus == InvitationStatus.SELF_INVITATION) return "Visible";
                else return "Collapsed";
            }
        }

        public string InvitationSentInfoVisibility {
            get {
                if (lastInvitationStatus == InvitationStatus.INVITATION_SENT) return "Visible";
                else return "Collapsed";
            }
        }

        public string InvitationsBoxVisibility {
            get {
                if (lastRecivedInvitation != null) return "Visible";
                else return "Collapsed";
            }
        }

        public string ConversationBoxVisibility {
            get {
                if (selectedFriend != null && activeConversation) return "Visible";
                else return "Collapsed";
            }
        }

        public string LastInvitationUsername {
            get {
                if (lastRecivedInvitation != null) return lastRecivedInvitation.sender;
                else return "";
            }
        }

        public string InvitationUsername {
            get { return invitationUsername; }
            set {
                if (value != invitationUsername) {
                    invitationUsername = value;
                    OnPropertyChanged(nameof(InvitationUsername));
                }
            }
        }

        public string MessageToSendText {
            get { return messageToSendText; }
            set {
                if (value != messageToSendText) {
                    messageToSendText = value;
                    OnPropertyChanged(nameof(MessageToSendText));
                }
            }
        }

        public FriendItem SelectedFriend {
            set {
                if (value != null && value != selectedFriend) {
                    selectedFriend = value;
                    if (loadConversationThread != null) loadConversationThread.Join();
                    string friendUsernameCopy = selectedFriend.Name;
                    loadConversationThread = new Thread(() => LoadConversationAsync(friendUsernameCopy));
                    loadConversationThread.Start();
                }
            }
        }

        public ICommand SendInvitationCommand {
            get {
                if (sendInvitationCommand == null) {
                    sendInvitationCommand = new RelayCommand(_ => {
                        string invitationUsernameCopy = invitationUsername;
                        sendInvitationThread = new Thread(() => SendInvitationAndGenerateDHKeysAsync(invitationUsernameCopy));
                        sendInvitationThread.Start();
                        invitationUsername = "";
                        OnPropertyChanged(nameof(InvitationUsername));
                    }, _ => {
                        if (lastInvitationStatus == InvitationStatus.NO_INVITATION && model.CheckUsernameText(invitationUsername)) return true;
                        else return false;
                    });
                }
                return sendInvitationCommand;
            }
        }

        public ICommand AcceptInvitationCommand {
            get {
                if (acceptInvitationCommand == null) {
                    acceptInvitationCommand = new RelayCommand(_ => {
                        acceptFriendThread = new Thread(AcceptFriendAsync);
                        acceptFriendThread.Start();
                    }, _ => {
                        if (lastRecivedInvitation != null) return true;
                        else return false;
                    });
                }
                return acceptInvitationCommand;
            }
        }

        public ICommand DeclineInvitationCommand {
            get {
                if (declineInvitationCommand == null) {
                    declineInvitationCommand = new RelayCommand(_ => {
                        declineFriendThread = new Thread(DeclineFriendAsync);
                        declineFriendThread.Start();
                    }, _ => {
                        if (lastRecivedInvitation != null) return true;
                        else return false;
                    });
                }
                return declineInvitationCommand;
            }
        }

        public ICommand SendMessageCommand {
            get {
                if (sendMessageCommand == null) {
                    sendMessageCommand = new RelayCommand(_ => {
                        string messageToSendTextCopy = messageToSendText;
                        string friendUsername = selectedFriend.Name;
                        sendMessageThread = new Thread(() => SendMessageAsync(friendUsername, messageToSendTextCopy));
                        sendMessageThread.Start();
                        messageToSendText = "";
                        OnPropertyChanged(nameof(MessageToSendText));
                    }, _ => {
                        if (messageToSendText.Length > 0) return true;
                        else return false;
                    });
                }
                return sendMessageCommand;
            }
        }

        public ICommand LogoutCommand {
            get {
                if (logoutCommand == null) {
                    logoutCommand = new RelayCommand(_ => {
                        keepUpdating = false;
                        if (sendInvitationThread != null && sendInvitationThread.IsAlive) sendInvitationThread.Join();
                        if (acceptFriendThread != null && acceptFriendThread.IsAlive) acceptFriendThread.Join();
                        if (declineFriendThread != null && declineFriendThread.IsAlive) declineFriendThread.Join();
                        if (loadConversationThread != null && loadConversationThread.IsAlive) loadConversationThread.Join();
                        if (sendInvitationThread != null && sendInvitationThread.IsAlive) sendInvitationThread.Join();
                        if (updateThread != null && updateThread.IsAlive) updateThread.Join();
                        model.Logout();
                        navigator.CurrentViewModel = new LoginViewModel(connection, navigator);
                    }, _ => true);
                }
                return logoutCommand;
            }
        }

        public ChatViewModel(ServerConnection connection, Navigator navigator, string username, byte[] userKey) : base(connection, navigator) {
            Log.SetLogFileName("log_" + username + ".log");

            this.model = new ChatModel(connection, username, userKey);
            this.lastInvitationStatus = InvitationStatus.NO_INVITATION;
            this.lastRecivedInvitation = null;
            this.selectedFriend = null;
            this.invitationUsername = "";
            this.messageToSendText = "";
            this.activeConversation = false;
            this.keepUpdating = true;
            updateThread = new Thread(UpdateAsync);
            updateThread.Start();
        }

        private void RefreshFriendInvitationMessage() {
            OnPropertyChanged(nameof(UserNotFoundErrorVisibility));
            OnPropertyChanged(nameof(UserAlreadyAFriendErrorVisibility));
            OnPropertyChanged(nameof(InvitationAlredyExistErrorVisibility));
            OnPropertyChanged(nameof(SelfInvitationErrorVisibility));
            OnPropertyChanged(nameof(InvitationSentInfoVisibility));
        }

        private void SendInvitationAndGenerateDHKeysAsync(string invitationUsernameCopy) {
            lastInvitationStatus = InvitationStatus.WAITING_FOR_RESPONSE;

            bool userExists = model.CheckUserExist(invitationUsernameCopy);

            if (!userExists) lastInvitationStatus = InvitationStatus.USER_NOT_FOUND;
            else {
                bool userAlredyAFriend = false;
                foreach (FriendItem f in model.Friends) {
                    if (f.Name == invitationUsernameCopy) userAlredyAFriend = true;
                }
                if (userAlredyAFriend) lastInvitationStatus = InvitationStatus.USER_ALREADY_A_FRIEND;
                else {
                    (InvitationStatus invitationStatus, string g, string p, string invitationID) = model.SendInvitation(invitationUsernameCopy);
                    Log.LogText("1sender p: " + p);
                    Log.LogText("1sender g: " + g);
                    Log.LogText("1sender invitationID: " + invitationID);
                    lastInvitationStatus = invitationStatus;
                    if (lastInvitationStatus == InvitationStatus.INVITATION_SENT) {
                        RefreshFriendInvitationMessage();
                        (string publicDHKey, byte[] privateDHKey) = model.GenerateDHKeys(p, g);
                        Log.LogText("1sender publicDHKey: " + publicDHKey);
                        Log.LogText("1sender privateDHKey: " + Security.ByteArrayToHexString(privateDHKey));
                        byte[] iv = Security.GenerateIV();
                        byte[] encryptedPrivateDHKey = Security.AESEncrypt(privateDHKey, model.UserKey, iv);
                        string IVHexString = Security.ByteArrayToHexString(iv);
                        Log.LogText("1sender iv: " + IVHexString);
                        string encryptedPrivateDHKeyHexString = Security.ByteArrayToHexString(encryptedPrivateDHKey);
                        Log.LogText("1sender encryptedPrivateDHKey: " + encryptedPrivateDHKeyHexString);
                        model.SendPublicDHKey(invitationID, publicDHKey, encryptedPrivateDHKeyHexString, IVHexString);
                    }
                }
            }

            if (lastInvitationStatus != InvitationStatus.INVITATION_SENT) RefreshFriendInvitationMessage();

            Thread.Sleep(3000);

            lastInvitationStatus = InvitationStatus.NO_INVITATION;

            RefreshFriendInvitationMessage();
        }

        private void AcceptFriendAsync() {
            string invitingPublicDHKey = lastRecivedInvitation.publicKeySender;
            Log.LogText("1reciver invitingPublicDHKey: " + invitingPublicDHKey);
            string p = lastRecivedInvitation.p;
            Log.LogText("1reciver p: " + p);
            string g = lastRecivedInvitation.g;
            Log.LogText("1reciver g: " + g);
            string invitationID = lastRecivedInvitation.invitationId.ToString();
            Log.LogText("1reciver invitationID: " + invitationID);
            model.ReceivedInvitations.RemoveAt(model.ReceivedInvitations.Count - 1);
            lastRecivedInvitation = null;
            OnPropertyChanged(nameof(InvitationsBoxVisibility));
            OnPropertyChanged(nameof(LastInvitationUsername));

            (string publicDHKey, byte[] privateDHKey) = model.GenerateDHKeys(p, g);
            Log.LogText("1reciver publicDHKey: " + publicDHKey);
            Log.LogText("1reciver privateDHKey: " + Security.ByteArrayToHexString(privateDHKey));
            byte[] conversationKey = model.GenerateConversationKey(invitingPublicDHKey, p, g, privateDHKey);
            Log.LogText("1reciver conversationKey: " + Security.ByteArrayToHexString(conversationKey));
            (string conversationID, byte[] conversationIV) = model.AcceptFriendInvitation(invitationID, publicDHKey);
            Log.LogText("1reciver conversationID: " + conversationID);
            Log.LogText("1reciver conversationIV: " + Security.ByteArrayToHexString(conversationIV));
            byte[] encryptedConversationKey = Security.AESEncrypt(conversationKey, model.UserKey, conversationIV); //Using userKey to encrypt conversationKey
            Log.LogText("1reciver encryptedConversationKey: " + Security.ByteArrayToHexString(encryptedConversationKey));
            model.SendEncryptedConversationKey(conversationID, encryptedConversationKey);

            if (model.ReceivedInvitations.Count > 0) lastRecivedInvitation = model.ReceivedInvitations[^1];
            OnPropertyChanged(nameof(InvitationsBoxVisibility));
            OnPropertyChanged(nameof(LastInvitationUsername));
        }

        private void DeclineFriendAsync() {
            string invitationID = lastRecivedInvitation.invitationId.ToString();
            model.ReceivedInvitations.RemoveAt(model.ReceivedInvitations.Count - 1);
            lastRecivedInvitation = null;
            OnPropertyChanged(nameof(InvitationsBoxVisibility));
            OnPropertyChanged(nameof(LastInvitationUsername));

            model.DeclineFriendInvitation(invitationID);

            if (model.ReceivedInvitations.Count > 0) lastRecivedInvitation = model.ReceivedInvitations[^1];
            OnPropertyChanged(nameof(InvitationsBoxVisibility));
            OnPropertyChanged(nameof(LastInvitationUsername));
        }

        private void ManageAcceptedFriends(List<ExtendedInvitation> acceptedInvitations) {
            if (acceptedInvitations != null && acceptedInvitations.Count > 0) {
                foreach (ExtendedInvitation inv in acceptedInvitations) {
                    Log.LogText("2sender encryptedSenderPrivateKey: " + inv.encryptedSenderPrivateKey);
                    byte[] encryptedPrivateDHKey = Security.HexStringToByteArray(inv.encryptedSenderPrivateKey);
                    Log.LogText("2sender IVToDecyptPrivateDHKey: " + inv.ivToDecryptSenderPrivateKey);
                    byte[] IVToDecyptPrivateDHKey = Security.HexStringToByteArray(inv.ivToDecryptSenderPrivateKey);
                    Log.LogText("2sender conversationIV: " + inv.conversationIv);
                    byte[] conversationIV = Security.HexStringToByteArray(inv.conversationIv);
                    byte[] privateDHKey = Security.AESDecrypt(encryptedPrivateDHKey, model.UserKey, IVToDecyptPrivateDHKey);
                    Log.LogText("2sender privateDHKey: " + Security.ByteArrayToHexString(privateDHKey));
                    byte[] conversationKey = model.GenerateConversationKey(inv.publicKeyReciver, inv.p, inv.g, privateDHKey);
                    Log.LogText("2sender conversationKey: " + Security.ByteArrayToHexString(conversationKey));
                    byte[] encryptedConversationKey = Security.AESEncrypt(conversationKey, model.UserKey, conversationIV); //Using userKey to encrypt conversationKey
                    Log.LogText("2sender encryptedConversationKey: " + Security.ByteArrayToHexString(encryptedConversationKey));
                    model.SendEncryptedConversationKey(inv.conversationId, encryptedConversationKey);
                }
            }
        }

        private void LoadConversationAsync(string friendUsernameCopy) {
            if (!model.Conversations.ContainsKey(friendUsernameCopy)) model.GetConversation(friendUsernameCopy);
            model.ActivateConversation(friendUsernameCopy);
            activeConversation = true;
            model.RemoveNotification(friendUsernameCopy);
            OnPropertyChanged(nameof(Friends));
            OnPropertyChanged(nameof(ConversationBoxVisibility));
            OnPropertyChanged(nameof(Messages));
        }

        private void SendMessageAsync(string friendUsernameCopy, string messageToSendTextCopy) {
            model.AddUserMessageToConversation(friendUsernameCopy, messageToSendTextCopy);
            OnPropertyChanged(nameof(Messages));
            string encryptedMessageToSendJSON = model.CreateEncryptedMessageToSendJSON(friendUsernameCopy, messageToSendTextCopy);
            model.SendMessage(friendUsernameCopy, encryptedMessageToSendJSON);
        }

        private void UpdateAsync() {
            int i = 1;
            while (keepUpdating) {
                if (i == 5) {
                    model.GetFriends();
                    OnPropertyChanged(nameof(Friends));
                    if (model.GetNotifications()) OnPropertyChanged(nameof(Friends));
                    if (model.GetInvitations()) {
                        OnPropertyChanged(nameof(InvitationsBoxVisibility));
                        OnPropertyChanged(nameof(LastInvitationUsername));
                    }
                    if (model.ReceivedInvitations.Count > 0) lastRecivedInvitation = model.ReceivedInvitations[^1]; //^1 - last item in the list
                    else lastRecivedInvitation = null;
                    ManageAcceptedFriends(model.GetAcceptedInvitations());
                }
                if (activeConversation) {
                    if (model.GetMessages(selectedFriend.Name)) OnPropertyChanged(nameof(Messages));
                }

                Thread.Sleep(baseRefreshRate);
                i++;
                if (i > 5) i = 1;
            }
        }
    }
}
