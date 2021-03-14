using Client.Commands;
using Client.Models;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private ChatModel model;

        private Thread sendInvitationThread;

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

        public List<Friend> FriendsList { get { return model.FriendsList; } }

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
                        sendInvitationThread = new Thread(SendInvitationAsync);
                        sendInvitationThread.Start();
                    }, _ => {
                        if (lastInvitationStatus == InvitationStatus.NO_INVITATION && model.CheckUsernameText(model.InvitationUsername)) return true;
                        else return false;
                    });
                }
                return sendInvitationCommand;
            }
        }

        public ChatViewModel(ServerConnection connection, Navigator navigator, string username) : base(connection, navigator) {
            this.model = new ChatModel(this.connection);
            this.model.Username = username;
            this.model.FriendsList = JsonConvert.DeserializeObject<List<Friend>>(this.model.GetFriendsJSON());
            this.lastInvitationStatus = InvitationStatus.NO_INVITATION;
            OnPropertyChanged(nameof(FriendsList));
        }

        private void SendInvitationAsync() {
            lastInvitationStatus = InvitationStatus.WAITING_FOR_RESPONSE;

            bool userExists = model.CheckUserExist(model.InvitationUsername);

            if (!userExists) lastInvitationStatus = InvitationStatus.USER_NOT_FOUND;
            else {
                bool userAlredyAFriend = false;
                foreach (Friend f in model.FriendsList) {
                    if (f.Username == model.InvitationUsername) userAlredyAFriend = true;
                }
                if (userAlredyAFriend) lastInvitationStatus = InvitationStatus.USER_ALREADY_A_FRIEND;
                else {
                    //Send invitation
                    lastInvitationStatus = InvitationStatus.INVITATION_SENT;
                }
            }

            OnPropertyChanged(nameof(UserNotFoundErrorVisibility));
            OnPropertyChanged(nameof(UserAlreadyAFriendErrorVisibility));
            OnPropertyChanged(nameof(InvitationSentInfoVisibility));

            Thread.Sleep(5000);

            model.InvitationUsername = "";
            lastInvitationStatus = InvitationStatus.NO_INVITATION;

            OnPropertyChanged(nameof(UserNotFoundErrorVisibility));
            OnPropertyChanged(nameof(UserAlreadyAFriendErrorVisibility));
            OnPropertyChanged(nameof(InvitationSentInfoVisibility));
            OnPropertyChanged(nameof(InvitationUsername));
        }
    }
}
