using Client.Commands;
using Client.Models;
using System;
using System.Windows.Input;

namespace Client.ViewModels 
{
    public class LoginViewModel : BaseViewModel
    {
        private LoginModel model;

        private RelayCommand loginCommand;
        private RelayCommand goRegistratonCommand;

        private bool goodUsername;
        private bool goodPass;
        private bool successRegistration;
        private bool loginError;

        public string Username {
            get { return model.Username; }
            set {
                if (value != model.Username) {
                    model.Username = value;
                    if (!String.IsNullOrEmpty(model.Username)) goodUsername = true;
                    else goodUsername = false;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string Pass {
            get { return model.Pass; }
            set {
                if (value != model.Pass) {
                    model.Pass = value;
                    if (!String.IsNullOrEmpty(model.Pass)) goodPass = true;
                    else goodPass = false;
                    OnPropertyChanged(nameof(Pass));
                }
            }
        }

        public string SuccessfulRegistrationMessageVisibility {
            get {
                if (successRegistration && !loginError) return "Visible";
                else return "Collapsed";
            }
        }

        public string LoginErrorMessageVisibility {
            get {
                if (loginError) return "Visible";
                else return "Collapsed";
            }
        }

        public ICommand LoginCommand {
            get {
                if (loginCommand == null) {
                    loginCommand = new RelayCommand(_ => {
                        string username = model.LoginUser(model.Username, model.Pass);
                        if (username != null) {
                            byte[] credentialsHash = model.CreateCredentialsHash(model.Username, model.Pass, "4372412310984218424902384480239489213245274532048723");
                            navigator.CurrentViewModel = new ChatViewModel(connection, navigator, username, credentialsHash);
                        }
                        else {
                            loginError = true;
                            OnPropertyChanged(nameof(SuccessfulRegistrationMessageVisibility));
                            OnPropertyChanged(nameof(LoginErrorMessageVisibility));
                        }
                    }, _ => {
                        if (goodUsername && goodPass) return true;
                        else return false;
                    });
                }
                return loginCommand;
            }
        }

        public ICommand GoRegistratonCommand {
            get {
                if (goRegistratonCommand == null) {
                    goRegistratonCommand = new RelayCommand(_ => {
                        navigator.CurrentViewModel = new RegisterViewModel(connection, navigator);
                    }, _ => true);
                }
                return goRegistratonCommand;
            }
        }

        public LoginViewModel(ServerConnection connection, Navigator navigator, bool successRegistration = false) : base(connection, navigator) {
            model = new LoginModel(connection);
            this.successRegistration = successRegistration;
            this.goodUsername = false;
            this.goodPass = false;
            this.loginError = false;
        }

    }
}
