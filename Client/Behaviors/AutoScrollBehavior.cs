using Client.Views;
using System.Windows;
using System.Windows.Controls;

namespace Client.Behaviors
{
    public static class AutoScrollBehavior
    {
        private static bool disableOnce = false;

        public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));

        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var scrollViewer = obj as ScrollViewer;
            if (scrollViewer != null && (bool)args.NewValue) {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                scrollViewer.ScrollToEnd();
            }
            else {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }

        public static void disableAutoScrollForOneOp()
        {
            disableOnce = true;
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if(disableOnce)
            {
                disableOnce = false;
                return;
            }
            if (e.ExtentHeightChange != 0 && ((e.ExtentHeight - e.ViewportHeight - 300) < e.VerticalOffset)) {
                var scrollViewer = sender as ScrollViewer;
                scrollViewer?.ScrollToBottom();
            } else
            {
                //ChatView.showNewMessageInfo(true);
            }
        }

        public static bool GetAutoScroll(DependencyObject obj) {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value) {
            obj.SetValue(AutoScrollProperty, value);
        }
    }
}

