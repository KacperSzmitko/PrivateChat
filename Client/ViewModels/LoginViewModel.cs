﻿using Client.Commands;
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
                        (byte[] userIV, byte[] userKeyHash) = model.LoginUser();
                        if (userIV != null && userKeyHash != null) {
                            byte[] credentialsHash = model.CreateCredentialsHash(userIV);
                            byte[] userKey = model.VerifyAndGetUserKey(credentialsHash, userIV, userKeyHash);
                            if (userKey == null) navigator.CurrentViewModel = new UserKeyInputViewModel(connection, navigator, model.Username, userKeyHash, userIV, credentialsHash);
                            else navigator.CurrentViewModel = new ChatViewModel(connection, navigator, model.Username, userKey);
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
            this.model = new LoginModel(this.connection);
            this.successRegistration = successRegistration;
            this.goodUsername = false;
            this.goodPass = false;
            this.loginError = false;
        }

    }
}
