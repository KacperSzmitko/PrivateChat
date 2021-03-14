using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView() {
            InitializeComponent();
        }

        private void PasswordBox1_PasswordChanged(object sender, RoutedEventArgs e) {
            if (this.DataContext != null) { ((RegisterViewModel)this.DataContext).Pass1 = ((PasswordBox)sender).Password; }
        }

        private void PasswordBox2_PasswordChanged(object sender, RoutedEventArgs e) {
            if (this.DataContext != null) { ((RegisterViewModel)this.DataContext).Pass2 = ((PasswordBox)sender).Password; }
        }
    }
}
