using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public class Navigator
    {
        private BaseViewModel currentViewModel;
        public BaseViewModel CurrentViewModel {
            get {
                return currentViewModel;
            }
            set {
                currentViewModel = value;
                OnViewChanged();
            }
        }

        public delegate void ViewChangedEventHandler(object source, EventArgs args);

        public event ViewChangedEventHandler ViewChanged;

        protected virtual void OnViewChanged() {
            if (ViewChanged != null) ViewChanged(this, EventArgs.Empty);
        }
    }
}
