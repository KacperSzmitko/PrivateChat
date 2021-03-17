using Client.Models;

namespace Client.ViewModels
{
    class UserKeyOutputViewModel : BaseViewModel
    {
        public UserKeyOutputViewModel(ServerConnection connection, Navigator navigator, string userKeyHexString) : base(connection, navigator) {
            UserKeyOutputModel model = new UserKeyOutputModel(this.connection, userKeyHexString);
        }
    }
}
