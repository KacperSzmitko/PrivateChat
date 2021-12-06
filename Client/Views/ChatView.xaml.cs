using Client.Behaviors;
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
    public partial class ChatView : UserControl
    {
        private static Boolean showLoadMore = false;
        private static Boolean loadingMore = false;
        private static double actualFullHeight = 0;
        public ChatView() {
            InitializeComponent();
        }

        private void ScrollViewer_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e) {

        }

        public static void showLoadMoreSet(Boolean showLoadMoreS)
        {
            showLoadMore = showLoadMoreS;
        }

        private void onScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = (ScrollViewer)sender;


            if (showLoadMore)
            {
                if (LoadingBorder.Visibility == Visibility.Collapsed)
                {
                    AutoScrollBehavior.disableAutoScrollForOneOp();
                    LoadingBorder.Visibility = Visibility.Visible;
                    sv.UpdateLayout();
                }
            }
            else
            {
                if (LoadingBorder.Visibility == Visibility.Visible)
                {
                    AutoScrollBehavior.disableAutoScrollForOneOp();
                    LoadingBorder.Visibility = Visibility.Collapsed;
                    sv.UpdateLayout();
                }
            }

            if (e.VerticalOffset < 100 && e.ExtentHeight > 100)
            {
                if (showLoadMore && loadingMore == false)
                {
                    actualFullHeight = sv.ExtentHeight;
                    loadingMore = true;
                    ChatViewModel.setLoadMore();
                }

                //AutoScrollBehavior.disableAutoScrollForOneOp();
                /*if (LoadingBorder.Visibility == Visibility.Collapsed)
                    LoadingBorder.Visibility = Visibility.Visible;
                else
                    LoadingBorder.Visibility = Visibility.Collapsed;*/
                //TODO: trigger load more data
            }

            if (loadingMore && (sv.ExtentHeight != actualFullHeight))
            {
                sv.ScrollToVerticalOffset(sv.ExtentHeight - actualFullHeight);
                loadingMore = false;
            }
            
        }

        private void onDataChanged(object sender, DependencyPropertyChangedEventArgs e) //TODO useless
        {
            ScrollViewer sv = (ScrollViewer)sender;

            

            if (showLoadMore)
            {
                if (LoadingBorder.Visibility == Visibility.Collapsed)
                {
                    AutoScrollBehavior.disableAutoScrollForOneOp();
                    LoadingBorder.Visibility = Visibility.Visible;
                    sv.UpdateLayout();
                }
            }
            else
            {
                if (LoadingBorder.Visibility == Visibility.Visible)
                {
                    AutoScrollBehavior.disableAutoScrollForOneOp();
                    LoadingBorder.Visibility = Visibility.Collapsed;
                    sv.UpdateLayout();
                }
            }
        }
    }
}
