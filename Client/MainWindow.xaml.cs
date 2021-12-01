using Client.ViewModels;
using System;
using System.Configuration;
using System.Windows;


namespace Client
{
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();
            //System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration("Client.config");
            ServerConnection connection = new ServerConnection(ConfigurationManager.AppSettings.Get("server-ip"), Convert.ToUInt16(ConfigurationManager.AppSettings.Get("server-port")), ConfigurationManager.AppSettings.Get("server-certname"));
            Navigator navigator = new Navigator();
            MainViewModel viewModel = new MainViewModel(connection, navigator);
            navigator.ViewChanged += viewModel.OnViewChanged;
            Closing += viewModel.OnWindowClosing;
            DataContext = viewModel;
            
        }
    }
}
