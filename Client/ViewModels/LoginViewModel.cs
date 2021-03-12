using Client.Commands;
using Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace Client.ViewModels 
{
    public class LoginViewModel : BaseViewModel
    {
        private LoginModel model;

        private RelayCommand loginCommand;
        private RelayCommand goRegistratonCommand;

        private string username;
        private string pass;
        private bool goodUsername;
        private bool goodPass;
        private bool successRegistration;
        private bool loginError;

        public string Username {
            get { return username; }
            set {
                if (value != username) {
                    username = value;
                    if (!String.IsNullOrEmpty(username)) goodUsername = true;
                    else goodUsername = false;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string Pass {
            get { return pass; }
            set {
                if (value != pass) {
                    pass = value;
                    if (!String.IsNullOrEmpty(pass)) goodPass = true;
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
                        User user = model.LoginUser(Username, Pass);
                        if (user != null) {
                            navigator.CurrentViewModel = new HomeViewModel(connection, navigator, user);
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
