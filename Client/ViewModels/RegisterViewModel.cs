using Client.Commands;
using Client.Models;
using Shared;
using System;
using System.Threading;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private RegisterModel model;

        private Thread updateIfUsernameExistThread;

        private RelayCommand registerCommand;
        private RelayCommand goLoginCommand;

        private bool goodUsername;
        private bool usernameAvailable;
        private bool goodPass1;
        private bool goodPass2;

        public string Username {
            get { return model.Username; }
            set {
                if (value != model.Username) {
                    model.Username = value;
                    if (model.CheckUsernameText()) goodUsername = true;
                    else goodUsername = false;
                    UpdateUsernameBox();
                    if (goodUsername) {
                        updateIfUsernameExistThread = new Thread(UpdateIfUsernameExistAsync);
                        updateIfUsernameExistThread.Start();
                    }
                }
            }
        }

        public string Pass1 {
            get { return model.Pass1; }
            set {
                if (value != model.Pass1) {
                    model.Pass1 = value;
                    if (model.CheckPasswordText()) goodPass1 = true;
                    else goodPass1 = false;
                    UpdatePass1Box();
                }
            }
        }

        public string Pass2 {
            get { return model.Pass2; }
            set {
                if (value != model.Pass2) {
                    model.Pass2 = value;
                    if (model.CheckPasswordsAreEqual()) goodPass2 = true;
                    else goodPass2 = false;
                    UpdatePass2Box();
                }
            }
        }

        public string UsernameBoxColor {
            get {
                if (String.IsNullOrEmpty(Username)) return "White";
                else if (!goodUsername || !usernameAvailable) return "Salmon";
                else return "LightGreen";
            }
        }

        public string Pass1BoxColor {
            get {
                if (String.IsNullOrEmpty(Pass1)) return "White";
                else if (!goodPass1) return "Salmon";
                else return "LightGreen";
            }
        }

        public string Pass2BoxColor {
            get {
                if (String.IsNullOrEmpty(Pass2)) return "White";
                else if (!goodPass2) return "Salmon";
                else return "LightGreen";
            }
        }

        public string UsernameInfoVisibility {
            get {
                if (!String.IsNullOrEmpty(Username) && !goodUsername) return "Visible";
                else return "Collapsed";
            }
        }

        public string UsernameInfoAvailabilityVisibility {
            get {
                if (!String.IsNullOrEmpty(Username) && goodUsername && !usernameAvailable) return "Visible";
                else return "Collapsed";
            }
        }

        public string Pass1InfoVisibility {
            get {
                if (!String.IsNullOrEmpty(Pass1) && !goodPass1) return "Visible";
                else return "Collapsed";
            }
        }

        public string Pass2InfoVisibility {
            get {
                if (!String.IsNullOrEmpty(Pass2) && !goodPass2) return "Visible";
                else return "Collapsed";
            }
        }
        
        public ICommand RegisterCommand {
            get {
                if (registerCommand == null) {
                    registerCommand = new RelayCommand(_ => {
                        (byte[] userKey, byte[] userIV) = Security.GenerateAESKeyAndIV();
                        byte[] userKeyHash = Security.CreateSHA256Hash(userKey);
                        if (model.RegisterUser(userIV, userKeyHash)) {
                            byte[] credentialsHash = model.CreateCredentialsHash(userIV);
                            model.SaveEncryptedUserKey(userKey, credentialsHash, userIV);
                            updateIfUsernameExistThread.Join();
                            navigator.CurrentViewModel = new UserKeyOutputViewModel(connection, navigator, Security.ByteArrayToHexString(userKey));
                        }
                    }, _ => {
                        if (goodUsername && usernameAvailable && goodPass1 && goodPass2) return true;
                        else return false;
                    });
                }
                return registerCommand;
            }
        }

        public ICommand GoLoginCommand {
            get {
                if (goLoginCommand == null) {
                    goLoginCommand = new RelayCommand(_ => {
                        if (updateIfUsernameExistThread != null) updateIfUsernameExistThread.Join();
                        navigator.CurrentViewModel = new LoginViewModel(connection, navigator);
                    }, _ => true);
                }
                return goLoginCommand;
            }
        }

        public RegisterViewModel(ServerConnection connection, Navigator navigator) : base(connection, navigator) {
            model = new RegisterModel(this.connection);
            this.goodUsername = false;
            this.usernameAvailable = true;
            this.goodPass1 = false;
            this.goodPass2 = false;
        }

        private void UpdateUsernameBox() {
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(UsernameBoxColor));
            OnPropertyChanged(nameof(UsernameInfoVisibility));
            OnPropertyChanged(nameof(UsernameInfoAvailabilityVisibility));
        }

        private void UpdatePass1Box() {
            OnPropertyChanged(nameof(Pass1BoxColor));
            OnPropertyChanged(nameof(Pass1InfoVisibility));
        }

        private void UpdatePass2Box() {
            OnPropertyChanged(nameof(Pass2BoxColor));
            OnPropertyChanged(nameof(Pass2InfoVisibility));
        }

        private void UpdateIfUsernameExistAsync() {
            bool exists = model.CheckUsernameExist();
            if (exists) usernameAvailable = false;
            else usernameAvailable = true;
            UpdateUsernameBox();
        }
    }
}
