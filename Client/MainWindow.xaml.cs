using Client.ViewModels;
using System.Windows;


namespace Client
{
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();
            ServerConnection connection = new ServerConnection("bd-projekt.ddns.net", 13579, "bd-projekt.ddns.net");
            Navigator navigator = new Navigator();
            MainViewModel viewModel = new MainViewModel(connection, navigator);
            navigator.ViewChanged += viewModel.OnViewChanged;
            Closing += viewModel.OnWindowClosing;
            DataContext = viewModel;
            
        }
    }
}
