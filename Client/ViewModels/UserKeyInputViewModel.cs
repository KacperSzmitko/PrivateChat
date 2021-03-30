using Client.Commands;
using Client.Models;
using System.Threading;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class UserKeyInputViewModel : BaseViewModel
    {
        UserKeyInputModel model;

        private RelayCommand checkUserKeyCommand;
        private RelayCommand goChatCommand;

        private bool keyChecked;
        private bool userKeyIsGood;

        public string UserKeyHexStringFromInput { 
            get { return model.UserKeyHexStringFromInput; }
            set {
                if (value != model.UserKeyHexStringFromInput) {
                    model.UserKeyHexStringFromInput = value;
                    OnPropertyChanged(UserKeyHexStringFromInput);
                }
            }
        }

        public string UserKeyOkMessageVisibility {
            get {
                if (userKeyIsGood && keyChecked) return "Visible";
                else return "Collapsed";
            }
        }

        public string UserKeyNotOkMessageVisibility {
            get {
                if (!userKeyIsGood && keyChecked) return "Visible";
                else return "Collapsed";
            }
        }

        public ICommand CheckUserKeyCommand {
            get {
                if (checkUserKeyCommand == null) {
                    checkUserKeyCommand = new RelayCommand(_ => {
                        keyChecked = true;
                        userKeyIsGood = model.IsUserKeyGood();
                        OnPropertyChanged(nameof(UserKeyOkMessageVisibility));
                        OnPropertyChanged(nameof(UserKeyNotOkMessageVisibility));
                    }, _ => true);
                }
                return checkUserKeyCommand;
            }
        }

        public ICommand GoChatCommand {
            get {
                if (goChatCommand == null) {
                    goChatCommand = new RelayCommand(_ => {
                        navigator.CurrentViewModel = new ChatViewModel(connection, navigator, model.Username, model.UserKey);
                    }, _ => {
                        if (userKeyIsGood) return true;
                        else return false;
                    });
                }
                return goChatCommand;
            }
        }


        public UserKeyInputViewModel(ServerConnection connection, Navigator navigator, string username, byte[] userKeyHash, byte[] userIV, byte[] credentialsHash) : base(connection, navigator) {
            this.model = new UserKeyInputModel(this.connection, username, userKeyHash, userIV, credentialsHash);
            this.userKeyIsGood = false;
            this.keyChecked = false;
        }
    }
}
