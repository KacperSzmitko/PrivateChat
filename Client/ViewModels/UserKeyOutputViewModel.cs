using Client.Commands;
using Client.Models;
using System.Threading;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class UserKeyOutputViewModel : BaseViewModel
    {
        UserKeyOutputModel model;

        private Thread attentionTimerThread;

        private RelayCommand goLoginCommand;

        private bool allowExit;

        public string UserKeyHexString { get { return model.UserKeyHexString; } }

        public ICommand GoLoginCommand {
            get {
                if (goLoginCommand == null) {
                    goLoginCommand = new RelayCommand(_ => {
                        navigator.CurrentViewModel = new LoginViewModel(connection, navigator, true);
                    }, _ => {
                        if (allowExit) return true;
                        else return false;
                    });
                }
                return goLoginCommand;
            }
        }

        public UserKeyOutputViewModel(ServerConnection connection, Navigator navigator, string userKeyHexString) : base(connection, navigator) {
            this.model = new UserKeyOutputModel(this.connection, userKeyHexString);
            this.allowExit = false;
            OnPropertyChanged(UserKeyHexString);
            attentionTimerThread = new Thread(AttentionTimerAsync);
            attentionTimerThread.Start();
        }

        private void AttentionTimerAsync() {
            Thread.Sleep(5000);
            allowExit = true;
        }
    }
}
