using System.Windows;
using System.Windows.Controls;

namespace Client.Components
{
    public class ButtonWithConfirmation : Button
    {
        public string Question { get; set; }

        protected override void OnClick() {
            if (string.IsNullOrWhiteSpace(Question)) {
                base.OnClick();
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show(Question, "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (messageBoxResult == MessageBoxResult.Yes) base.OnClick();
        }
    }
}
