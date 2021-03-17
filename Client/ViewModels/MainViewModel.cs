using Client.Models;
using System;

namespace Client.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private MainModel model;

        public BaseViewModel CurrentViewModel {
            get {
                return navigator.CurrentViewModel;
            }
            set {
                navigator.CurrentViewModel = value;
            }
        }

        public MainViewModel(ServerConnection connection, Navigator navigator) : base(connection, navigator) {
            this.model = new MainModel(connection);
            this.CurrentViewModel = new LoginViewModel(this.connection, this.navigator);
        }

        public void OnViewChanged(object source, EventArgs args) {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public void OnWindowClosing(object source, EventArgs args) {
            model.Disconnect();
        }

    }
}
