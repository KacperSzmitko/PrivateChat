using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();
            ServerConnection connection = new ServerConnection("localhost", 13579);
            Navigator navigator = new Navigator();
            MainViewModel viewModel = new MainViewModel(connection, navigator);
            navigator.ViewChanged += viewModel.OnViewChanged;
            Closing += viewModel.OnWindowClosing;
            DataContext = viewModel;
            
        }
    }
}
