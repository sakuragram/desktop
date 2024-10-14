using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using TdLib;

namespace sakuragram
{
	public sealed partial class MainWindow : Window
	{
		private static TdClient _client = App._client;
		private static TdApi.User _user;
		private static TdApi.ChatFolderInfo[] _folders = App._folders;

		private NavigationViewItem _lastItem;
		private int _totalUnreadCount;
		
		public MainWindow()
		{
			InitializeComponent();
			TitleBar.Title = Config.AppName;
			TitleBar.Icon = new BitmapIcon 
			{
				UriSource = new Uri("ms-appx:///Assets/Light.svg"), 
				ShowAsMonochrome = false
			};
			
			#if DEBUG
			{
				Title = $"{Config.AppName} debug";
				TitleBar.Subtitle = "debug";
			}
			#elif BETA
			{
				Title = Config.AppName + " beta";
				TitleBar.Subtitle = $"beta ({Config.AppVersion}, TdLib {Config.TdLibVersion})";
			}
			#elif RELEASE
			{
				Title = Config.AppName;
				TitleBar.Subtitle = Config.AppVersion;
			}
			#else
			{
				Title = Config.AppName;
				TitleBar.Subtitle = "UNTITLED CONFIGURATION";
			}
			#endif
			
            ExtendsContentIntoTitleBar = true;
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            NavigateToView("ChatsView", null);
            TrySetDesktopAcrylicBackdrop();
            
            var chatsIds = _client.ExecuteAsync(new TdApi.GetChats{Limit = 100}).Result.ChatIds;
            foreach (var chatId in chatsIds)
            {
	            var chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId}).Result;
	            _totalUnreadCount += chat.UnreadCount;
            }
            
			UnreadMessagesCount.Value = _totalUnreadCount;
			
			_user = _client.GetMeAsync().Result;
			NavigationView.PaneTitle = $"{_user.FirstName} ({_totalUnreadCount})";
			
			foreach (var chatFolderInfo in _folders)
			{
				try
				{
					string path = AppContext.BaseDirectory +
					              $@"Assets\icons\folders\folders_{chatFolderInfo.Icon.Name.ToLower()}.png";
					BitmapIcon folderIcon = new();
					
					if (File.Exists(path))
					{
						Uri folderIconPath = new(path);
						
						folderIcon.UriSource = folderIconPath;
						folderIcon.ShowAsMonochrome = false;
					}

					var folderItem = new NavigationViewItem();
					folderItem.Icon = folderIcon != null ? folderIcon : null;
					folderItem.Content = chatFolderInfo.Title;
					folderItem.Tag = "ChatsView";
					folderItem.Name = $"{chatFolderInfo.Title}_{chatFolderInfo.Id}";

					NavigationView.MenuItems.Add(folderItem);
				}
				catch (UriFormatException e)
				{
					Debug.WriteLine($"Error: {e.Message})");
					throw;
				}
			}
			
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
		}

		private Task ProcessUpdates(TdApi.Update update)
		{
			switch (update)
			{
				case TdApi.Update.UpdateNewMessage updateNewMessage:
				{
					_totalUnreadCount += 1;
					NavigationView.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
						() => NavigationView.PaneTitle = $"{_user.FirstName} ({_totalUnreadCount})");
					UnreadMessagesCount.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
						() => UnreadMessagesCount.Value = _totalUnreadCount);
					break;
				}
				case TdApi.Update.UpdateConnectionState updateConnectionState:
				{
					DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
					{
						NavigationView.PaneTitle = updateConnectionState.State switch
						{
							TdApi.ConnectionState.ConnectionStateReady => $"{_user.FirstName} ({_totalUnreadCount})",
							TdApi.ConnectionState.ConnectionStateUpdating => "Updating...",
							TdApi.ConnectionState.ConnectionStateConnecting => "Connecting...",
							TdApi.ConnectionState.ConnectionStateWaitingForNetwork => "Waiting for network...",
							TdApi.ConnectionState.ConnectionStateConnectingToProxy => "Connecting to proxy...",
							_ => Config.AppName
						};
					});
					break;
				}
			}

			return Task.CompletedTask;
		}

		private bool TrySetDesktopAcrylicBackdrop()
		{
			if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
			{
				Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
				SystemBackdrop = DesktopAcrylicBackdrop;

				return true;
			}

			return false;
		}
		
		private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) 
		{

		}

		private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer as NavigationViewItem;
			if (item == null || item == _lastItem)
				return;

			var clickedView = item.Tag.ToString();

			if (!NavigateToView(clickedView, item)) return;
			_lastItem = item;
		}

		private bool NavigateToView(string clickedView, NavigationViewItem item)
		{
			var view = Assembly.GetExecutingAssembly().GetType($"{Config.AppName}.Views.{clickedView}");

			if (string.IsNullOrEmpty(clickedView) || view == null)
				return false;

			ContentFrame.Navigate(view, null, new EntranceNavigationTransitionInfo());

			if (clickedView == "ChatsView" && item != null)
			{
				foreach (var folder in _folders)
				{
					if (item.Name == $"{folder.Title}_{folder.Id}")
					{
						App._folderId = folder.Id;
						break;
					}
					else if (item.Name == "NavViewChats")
					{
						App._folderId = -1;
						break;
					}
					else
					{
						continue;
					}
				}
			}

			NavigationView.PaneDisplayMode = clickedView switch
			{
				"SettingsView" => NavigationViewPaneDisplayMode.LeftCompact,
				"ChatsView" => NavigationViewPaneDisplayMode.Left,
				_ => NavigationView.PaneDisplayMode
			};

			return true;
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{

		}
    }
}
