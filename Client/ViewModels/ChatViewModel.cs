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
        private RelayCommand deleteAccountCommand;

        private InvitationStatuses lastInvitationStatus;
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
                if (lastInvitationStatus == InvitationStatuses.USER_NOT_FOUND) return "Visible";
                else return "Collapsed";
            }
        }

        public string UserAlreadyAFriendErrorVisibility {
            get {
                if (lastInvitationStatus == InvitationStatuses.USER_ALREADY_A_FRIEND) return "Visible";
                else return "Collapsed";
            }
        }

        public string InvitationAlredyExistErrorVisibility {
            get {
                if (lastInvitationStatus == InvitationStatuses.INVITATION_ALREADY_EXIST) return "Visible";
                else return "Collapsed";
            }
        }

        public string SelfInvitationErrorVisibility {
            get {
                if (lastInvitationStatus == InvitationStatuses.SELF_INVITATION) return "Visible";
                else return "Collapsed";
            }
        }

        public string InvitationSentInfoVisibility {
            get {
                if (lastInvitationStatus == InvitationStatuses.INVITATION_SENT) return "Visible";
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
                        if (lastInvitationStatus == InvitationStatuses.NO_INVITATION && model.CheckUsernameText(invitationUsername)) return true;
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
                        WaitForThreadsToEnd();
                        model.Logout();
                        navigator.CurrentViewModel = new LoginViewModel(connection, navigator);
                    }, _ => true);
                }
                return logoutCommand;
            }
        }

        public ICommand DeleteAccountCommand {
            get {
                if (deleteAccountCommand == null) {
                    deleteAccountCommand = new RelayCommand(_ => {
                        keepUpdating = false;
                        WaitForThreadsToEnd();
                        model.DeleteAccount();
                        navigator.CurrentViewModel = new LoginViewModel(connection, navigator);
                    }, _ => true);
                }
                return deleteAccountCommand;
            }
        }

        public ChatViewModel(ServerConnection connection, Navigator navigator, string username, byte[] userKey) : base(connection, navigator) {
            this.model = new ChatModel(connection, username, userKey);
            this.lastInvitationStatus = InvitationStatuses.NO_INVITATION;
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
            lastInvitationStatus = InvitationStatuses.WAITING_FOR_RESPONSE;

            bool userExists = model.CheckUserExist(invitationUsernameCopy);

            if (!userExists) lastInvitationStatus = InvitationStatuses.USER_NOT_FOUND;
            else {
                bool userAlredyAFriend = false;
                foreach (FriendItem f in model.Friends) {
                    if (f.Name == invitationUsernameCopy) userAlredyAFriend = true;
                }
                if (userAlredyAFriend) lastInvitationStatus = InvitationStatuses.USER_ALREADY_A_FRIEND;
                else {
                    (InvitationStatuses invitationStatus, string g, string p, string invitationID) = model.SendInvitation(invitationUsernameCopy);
                    lastInvitationStatus = invitationStatus;
                    if (lastInvitationStatus == InvitationStatuses.INVITATION_SENT) {
                        RefreshFriendInvitationMessage();
                        (string publicDHKey, byte[] privateDHKey) = model.GenerateDHKeys(p, g);
                        byte[] iv = Security.GenerateIV();
                        byte[] encryptedPrivateDHKey = Security.AESEncrypt(privateDHKey, model.UserKey, iv);
                        string IVHexString = Security.ByteArrayToHexString(iv);
                        string encryptedPrivateDHKeyHexString = Security.ByteArrayToHexString(encryptedPrivateDHKey);
                        model.SendPublicDHKey(invitationID, publicDHKey, encryptedPrivateDHKeyHexString, IVHexString);
                    }
                }
            }

            if (lastInvitationStatus != InvitationStatuses.INVITATION_SENT) RefreshFriendInvitationMessage();

            Thread.Sleep(3000);

            lastInvitationStatus = InvitationStatuses.NO_INVITATION;

            RefreshFriendInvitationMessage();
        }

        private void AcceptFriendAsync() {
            string invitingPublicDHKey = lastRecivedInvitation.publicKeySender;
            string p = lastRecivedInvitation.p;
            string g = lastRecivedInvitation.g;
            string invitationID = lastRecivedInvitation.invitationId.ToString();
            model.ReceivedInvitations.RemoveAt(model.ReceivedInvitations.Count - 1);
            lastRecivedInvitation = null;
            OnPropertyChanged(nameof(InvitationsBoxVisibility));
            OnPropertyChanged(nameof(LastInvitationUsername));

            (string publicDHKey, byte[] privateDHKey) = model.GenerateDHKeys(p, g);
            byte[] conversationKey = model.GenerateConversationKey(invitingPublicDHKey, p, g, privateDHKey);
            (string conversationID, byte[] conversationIV) = model.AcceptFriendInvitation(invitationID, publicDHKey);
            byte[] encryptedConversationKey = Security.AESEncrypt(conversationKey, model.UserKey, conversationIV); //Using userKey to encrypt conversationKey
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
                    byte[] encryptedPrivateDHKey = Security.HexStringToByteArray(inv.encryptedSenderPrivateKey);
                    byte[] IVToDecyptPrivateDHKey = Security.HexStringToByteArray(inv.ivToDecryptSenderPrivateKey);
                    byte[] conversationIV = Security.HexStringToByteArray(inv.conversationIv);
                    byte[] privateDHKey = Security.AESDecrypt(encryptedPrivateDHKey, model.UserKey, IVToDecyptPrivateDHKey);
                    byte[] conversationKey = model.GenerateConversationKey(inv.publicKeyReciver, inv.p, inv.g, privateDHKey);
                    byte[] encryptedConversationKey = Security.AESEncrypt(conversationKey, model.UserKey, conversationIV); //Using userKey to encrypt conversationKey
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
            int lastMessageIndex = model.AddUserMessageToConversation(friendUsernameCopy, messageToSendTextCopy);
            model.Conversations[friendUsernameCopy].Messages[lastMessageIndex].MessageStatus = MessageStatuses.MESSAGE_SENT;
            OnPropertyChanged(nameof(Messages));
            string encryptedMessageToSendJSON = model.CreateEncryptedMessageToSendJSON(friendUsernameCopy, messageToSendTextCopy);
            model.SendMessage(friendUsernameCopy, encryptedMessageToSendJSON);
            model.Conversations[friendUsernameCopy].Messages[lastMessageIndex].MessageStatus = MessageStatuses.MESSAGE_RECEIVED_SERVER;
            OnPropertyChanged(nameof(Messages));
        }

        public void WaitForThreadsToEnd() {
            if (sendInvitationThread != null && sendInvitationThread.IsAlive) sendInvitationThread.Join();
            if (acceptFriendThread != null && acceptFriendThread.IsAlive) acceptFriendThread.Join();
            if (declineFriendThread != null && declineFriendThread.IsAlive) declineFriendThread.Join();
            if (loadConversationThread != null && loadConversationThread.IsAlive) loadConversationThread.Join();
            if (sendInvitationThread != null && sendInvitationThread.IsAlive) sendInvitationThread.Join();
            if (updateThread != null && updateThread.IsAlive) updateThread.Join();
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
                    if (model.GetMessages(selectedFriend.Name)) {
                        OnPropertyChanged(nameof(Messages));
                    }
                }

                Thread.Sleep(baseRefreshRate);
                i++;
                if (i > 5) i = 1;
            }
        }
    }
}
