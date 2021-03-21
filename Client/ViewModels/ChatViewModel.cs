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
        private Thread updateThread;

        private RelayCommand sendInvitationCommand;

        private InvitationStatus lastInvitationStatus;

        public string Username { get { return model.Username; } }

        public string InvitationUsername {
            get { return model.InvitationUsername; }
            set {
                if (value != model.InvitationUsername) {
                    model.InvitationUsername = value;
                    OnPropertyChanged(nameof(InvitationUsername));
                }
            }
        }

        public ObservableCollection<FriendStatus> Friends { get { return new ObservableCollection<FriendStatus>(model.Friends); } }

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
        public string InvitationSentInfoVisibility {
            get {
                if (lastInvitationStatus == InvitationStatus.INVITATION_SENT) return "Visible";
                else return "Collapsed";
            }
        }


        public ICommand SendInvitationCommand {
            get {
                if (sendInvitationCommand == null) {
                    sendInvitationCommand = new RelayCommand(_ => {
                        sendInvitationThread = new Thread(SendInvitationAndGenerateDHKeysAsync);
                        sendInvitationThread.Start();
                    }, _ => {
                        if (lastInvitationStatus == InvitationStatus.NO_INVITATION && model.CheckUsernameText(model.InvitationUsername)) return true;
                        else return false;
                    });
                }
                return sendInvitationCommand;
            }
        }

        public ChatViewModel(ServerConnection connection, Navigator navigator, string username, byte[] userKey, byte[] userIV, byte[] credentialsHash) : base(connection, navigator) {
            this.model = new ChatModel(connection, username, userKey, userIV, credentialsHash);
            this.lastInvitationStatus = InvitationStatus.NO_INVITATION;
            updateThread = new Thread(UpdateAsync);
            updateThread.Start();
        }

        private void RefreshFriendInvitationMessage() {
            OnPropertyChanged(nameof(UserNotFoundErrorVisibility));
            OnPropertyChanged(nameof(UserAlreadyAFriendErrorVisibility));
            OnPropertyChanged(nameof(InvitationSentInfoVisibility));
        }

        private void SendInvitationAndGenerateDHKeysAsync() {
            lastInvitationStatus = InvitationStatus.WAITING_FOR_RESPONSE;

            bool userExists = model.CheckUserExist(model.InvitationUsername);

            if (!userExists) lastInvitationStatus = InvitationStatus.USER_NOT_FOUND;
            else {
                bool userAlredyAFriend = false;
                foreach (FriendStatus f in model.Friends) {
                    if (f.Name == model.InvitationUsername) userAlredyAFriend = true;
                }
                if (userAlredyAFriend) lastInvitationStatus = InvitationStatus.USER_ALREADY_A_FRIEND;
                else {
                    var (g, p, invitationID) = model.SendInvitation(Username);
                    lastInvitationStatus = InvitationStatus.INVITATION_SENT;
                    RefreshFriendInvitationMessage();
                    var (publicDHKey, privateDHKey) = model.GenerateDHKeys(g, p);
                    byte[] encryptedPrivateDHKey = Security.AESEncrypt(privateDHKey, model.CredentialsHash, model.UserIV);
                    model.SaveEncryptedPrivateDHKey(invitationID, encryptedPrivateDHKey);
                    model.SendPublicDHKey(invitationID, publicDHKey);
                }
            }

            if (lastInvitationStatus != InvitationStatus.INVITATION_SENT) RefreshFriendInvitationMessage();

            Thread.Sleep(10000);

            model.InvitationUsername = "";
            lastInvitationStatus = InvitationStatus.NO_INVITATION;

            RefreshFriendInvitationMessage();
            OnPropertyChanged(nameof(InvitationUsername));
        }

        private void UpdateAsync() {
            while (true) {
                model.GetFriends();
                model.GetNotifications();
                OnPropertyChanged(nameof(Friends));
                Thread.Sleep(1000);
            }
        }
    }
}
