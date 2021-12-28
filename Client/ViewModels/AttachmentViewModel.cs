using Client.Commands;
using Client.Models;
using Microsoft.Win32;
using Shared;
using System;
using System.Threading;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class AttachmentViewModel : BaseViewModel
    {
        private RelayCommand goLoginCommand;
        private RelayCommand openAttachmentCommand;

        public string imageURI;
        public string ImageURI
        {
            get
            {
                return imageURI;
            }
            set
            {
                imageURI = value;
                this.OnPropertyChanged("ImageURI");
            }
        }
        public ICommand GoBackCommand {
            get {
                if (goLoginCommand == null) {
                    goLoginCommand = new RelayCommand(_ => {
                        navigator.CurrentViewModel = new LoginViewModel(connection, navigator);
                    }, _ => true);
                }
                return goLoginCommand;
            }
        }

        public ICommand OpenAttachmentCommand
        {
            get
            {
                //Jeśli komenda jest równa null
                if (openAttachmentCommand == null)
                {
                    openAttachmentCommand = new RelayCommand(_ => {
                        navigator.CurrentViewModel = new AttachmentViewModel(connection, navigator);
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            imageURI = openFileDialog.FileName;
                            OnPropertyChanged(nameof(imageURI));
                        };

                    });
                }
                //Zwróć obiekt RelayCommand
                return openAttachmentCommand;
            }
        }

        public AttachmentViewModel(ServerConnection connection, Navigator navigator) : base(connection, navigator) {
            this.imageURI = "";
        }
    }
}
