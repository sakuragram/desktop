using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using TdLib;

namespace sakuragram.Views
{
    public sealed partial class SettingsView : Page
    {
        private TdClient _client = App._client;
        private ApplicationDataContainer _localSettings;
        private NavigationViewItem _lastItem;
        
        public SettingsView()
        {
            InitializeComponent();
            
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            NavigateToView("Profile");
            
            try
            {
                if (ApplicationData.Current.LocalSettings.Values[Config.AppName] != null)
                {
                    Debug.WriteLine(ApplicationData.Current.LocalFolder.Name + ", " + ApplicationData.Current.LocalFolder.Path);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            if (item == null || item == _lastItem)
                return;

            var clickedView = item.Tag.ToString();

            if (!NavigateToView(clickedView)) return;
            _lastItem = item;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
        }
        
        private bool NavigateToView(string clickedView)
        {
            var view = Assembly.GetExecutingAssembly().GetType($"sakuragram.Views.Settings.{clickedView}");

            if (string.IsNullOrEmpty(clickedView) || view == null)
                return false;

            ContentFrame.Navigate(view, null, new EntranceNavigationTransitionInfo());
            return true;
        }
    }
}
