using Client.Commands;
using Client.Models;
using Microsoft.Win32;
using Shared;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class AttachmentViewModel : BaseViewModel
    {
        private ChatModel model;
        private AttachmentModel attachmentModel;
        private Conversation actConversation;

        private const int baseRefreshRate = 150;

        private Boolean keepUpdating = true;
        private const long MAX_FILE_SIZE = 3 * 1024 * 1024;

        private Thread sendAttachmentThread;

        private RelayCommand goLoginCommand;
        private RelayCommand openAttachmentCommand;
        ObservableCollection<Attachment> attachments = new ObservableCollection<Attachment>();

        public ObservableCollection<Attachment> Attachments
        {
            get
            {
                return new ObservableCollection<Attachment>(attachmentModel.Attachments);
            }
        }

        public string fileURI;
        public string FileURI
        {
            get
            {
                return fileURI;
            }
            set
            {
                fileURI = value;
                this.OnPropertyChanged("ImageURI");
            }
        }
        public ICommand GoBackCommand
        {
            get
            {
                if (goLoginCommand == null)
                {
                    goLoginCommand = new RelayCommand(_ =>
                    {
                        navigator.CurrentViewModel = new ChatViewModel(connection, navigator, model.Username, model.UserKey);
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
                    openAttachmentCommand = new RelayCommand(_ =>
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            fileURI = openFileDialog.FileName;
                            OnPropertyChanged(nameof(fileURI));
                            String filename = openFileDialog.SafeFileName;

                            FileInfo file = new FileInfo(fileURI);
                            if (file.Exists)
                            {
                                long size = file.Length;

                                if (size > MAX_FILE_SIZE)
                                {
                                    //TODO: zbyt duży plik
                                }
                                else
                                {
                                    attachmentModel.Attachments.Add(new Attachment(filename));

                                    Byte[] fileBytes = File.ReadAllBytes(fileURI);
                                    String fileBase64 = Convert.ToBase64String(fileBytes);

                                    sendAttachmentThread = new Thread(() => SendAttachmentAsync(actConversation.ConversationID, fileBase64, filename));
                                    sendAttachmentThread.Start();

                                    OnPropertyChanged("Attachments");
                                }
                            }
                        };

                    });
                }
                //Zwróć obiekt RelayCommand
                return openAttachmentCommand;
            }
        }

        private void SendAttachmentAsync(string conversationID, string fileBytesBase64, string filename)
        {
            var res = model.SendAttachment(conversationID, fileBytesBase64, filename);

            foreach (var at in attachmentModel.Attachments)
            {
                if (at.fStatus == FILE_STATUS.NOT_SENT && at.Name.Equals(filename))
                {
                    at.attachmentID = res.attachmentID;
                    at.fStatus = FILE_STATUS.SENT;
                    break;
                }
            }
            OnPropertyChanged("Attachments");
        }

        public AttachmentViewModel(ServerConnection connection, Navigator navigator, string username, byte[] userKey, Conversation conv) : base(connection, navigator)
        {
            keepUpdating = true;
            this.fileURI = "";
            this.attachmentModel = new AttachmentModel(connection);
            this.model = new ChatModel(connection, username, userKey);
            this.actConversation = conv;
            keepUpdating = true;

            var allAttachments = new Thread(() => attachmentModel.GetAttachments(actConversation.ConversationID, this));
            allAttachments.Start();

            var updater = new Thread(() => UpdateAsync());
            updater.Start();
        }

        ~AttachmentViewModel()
        {
            keepUpdating = false;
        }

        private void UpdateAsync()
        {
            while (keepUpdating)
            {
                if (attachmentModel.GetNewAttachments(actConversation.ConversationID))
                {
                    OnPropertyChanged("Attachments");
                }

                Thread.Sleep(baseRefreshRate);
            }
        }

        public void OnPropertyChangedUp(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
