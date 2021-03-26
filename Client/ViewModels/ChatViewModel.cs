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

        private InvitationStatus lastInvitationStatus;
        private Invitation lastRecivedInvitation;
        private FriendItem selectedFriend;

        private string invitationUsername;
        private string messageToSendText;
        private bool activeConversation;

        public string Username { get { return model.Username; } }

        public ObservableCollection<FriendItem> Friends { get { return new ObservableCollection<FriendItem>(model.Friends); } }
        public ObservableCollection<MessageItem> Messages { get { return new ObservableCollection<MessageItem>(model.Conversations[selectedFriend.Name].Messages); } }

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
                selectedFriend = value;
                if (loadConversationThread != null) loadConversationThread.Join();
                loadConversationThread = new Thread(() => LoadConversationAsync(selectedFriend.Name));
                loadConversationThread.Start();
            }
        }

        public ICommand SendInvitationCommand {
            get {
                if (sendInvitationCommand == null) {
                    sendInvitationCommand = new RelayCommand(_ => {
                        sendInvitationThread = new Thread(() => SendInvitationAndGenerateDHKeysAsync(invitationUsername));
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
                        sendMessageThread = new Thread(() => SendMessageAsync(messageToSendText, selectedFriend.Name));
                        sendMessageThread.Start();
                        invitationUsername = "";
                        OnPropertyChanged(nameof(MessageToSendText));
                    }, _ => {
                        if (messageToSendText.Length > 0) return true;
                        else return false;
                    });
                }
                return sendMessageCommand;
            }
        }

        public ChatViewModel(ServerConnection connection, Navigator navigator, string username, byte[] userKey) : base(connection, navigator) {
            this.model = new ChatModel(connection, username, userKey);
            this.lastInvitationStatus = InvitationStatus.NO_INVITATION;
            this.lastRecivedInvitation = null;
            this.selectedFriend = null;
            this.activeConversation = false;
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
                    lastInvitationStatus = invitationStatus;
                    if (lastInvitationStatus == InvitationStatus.INVITATION_SENT) {
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

            if (lastInvitationStatus != InvitationStatus.INVITATION_SENT) RefreshFriendInvitationMessage();

            Thread.Sleep(3000);

            lastInvitationStatus = InvitationStatus.NO_INVITATION;

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
            if (!model.Conversations.ContainsKey(selectedFriend.Name)) model.GetConversation(selectedFriend.Name);
            model.ActivateConversation(selectedFriend.Name);
            activeConversation = true;
            OnPropertyChanged(nameof(ConversationBoxVisibility));
            OnPropertyChanged(nameof(Messages));
        }

        private void SendMessageAsync(string messageToSendTextCopy, string friendUsernameCopy) {
            string encryptedMessageToSendJSON = model.CreateEncryptedMessageToSendJSON(friendUsernameCopy, messageToSendTextCopy);
            model.SendMessage(friendUsernameCopy, encryptedMessageToSendJSON);
        }

        private void UpdateAsync() {
            while (true) {
                model.GetFriends();
                model.GetNotifications();
                model.GetInvitations();
                if (model.ReceivedInvitations.Count > 0) lastRecivedInvitation = model.ReceivedInvitations[^1]; //^1 - last item in the list
                else lastRecivedInvitation = null;
                ManageAcceptedFriends(model.GetAcceptedInvitations());
                if (activeConversation) model.GetMessages(selectedFriend.Name);

                OnPropertyChanged(nameof(Friends));
                OnPropertyChanged(nameof(InvitationsBoxVisibility));
                OnPropertyChanged(nameof(LastInvitationUsername));
                if (activeConversation) OnPropertyChanged(nameof(Messages));

                Thread.Sleep(500);
            }
        }

        
    }
}
