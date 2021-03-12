using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using Client.Commands;

namespace Client.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected ServerConnection connection;
        protected Navigator navigator;

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseViewModel(ServerConnection connection, Navigator navigator) {
            this.connection = connection;
            this.navigator = navigator;
        }

        protected void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
