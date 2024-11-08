using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using ABI.Microsoft.UI;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Octokit;
using sakuragram.Services.Core;
using TdLib;
using DispatcherQueuePriority = Microsoft.UI.Dispatching.DispatcherQueuePriority;

namespace sakuragram
{
	public sealed partial class MainWindow : Window
	{
		private static TdClient _client = App._client;
		private static TdApi.User _user;
		private static TdApi.ChatFolderInfo[] _folders = App._folders;

		private NavigationViewItem _lastItem;
		private int _totalUnreadCount;
		private string _updateFilePath;
		private bool _updateAvailable;
		
		private UpdateManager _updateManager = App.UpdateManager;
		private GitHubClient _gitHubClient = App._githubClient;
		public static ApplicationDataContainer _localSettings;
		
		public MainWindow()
		{
			InitializeComponent();
			TitleBar.Title = Config.AppName;
			TitleBar.Icon = new BitmapIcon 
			{
				UriSource = new Uri("ms-appx:///Assets/StoreLogo.scale-400.png"), 
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
            AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
            TrySetDesktopAcrylicBackdrop();

			CheckForUpdates();
			
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
            NotificationService notificationService = new();

            var chatsView = Assembly.GetExecutingAssembly().GetType($"{Config.AppName}.Views.ChatsView");
            ContentFrame.Navigate(chatsView);
		}

		private async void CheckForUpdates()
		{
			if (await _updateManager.CheckForUpdates())
			{
				var update = await _gitHubClient.Repository.Release.GetLatest(Config.GitHubRepoOwner, Config.GitHubRepoName);

				if (update.PublishedAt != null)
					TextBlockNewVersion.Text = update.Prerelease
						? $"Pre-release {update.TagName}"
						: $"Release {update.TagName}" + $", {update.PublishedAt.Value:dd.MM.yyyy}";
				TextBlockPatchNotes.Text = update.Body;
				ContentDialogNewVersion.ShowAsync();
			}
		}
		
		private Task ProcessUpdates(TdApi.Update update)
		{
			switch (update)
			{
				case TdApi.Update.UpdateNewMessage updateNewMessage:
				{
					_totalUnreadCount += 1;
					//DispatcherQueue.EnqueueAsync(() => NavigationView.PaneTitle = $"{_user.FirstName} ({_totalUnreadCount})");
					//DispatcherQueue.EnqueueAsync(() => UnreadMessagesCount.Value = _totalUnreadCount);
					break;
				}
				case TdApi.Update.UpdateConnectionState updateConnectionState:
				{
					DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
					{
						// NavigationView.PaneTitle = updateConnectionState.State switch
						// {
						// 	TdApi.ConnectionState.ConnectionStateReady => $"{_user.FirstName} ({_totalUnreadCount})",
						// 	TdApi.ConnectionState.ConnectionStateUpdating => "Updating...",
						// 	TdApi.ConnectionState.ConnectionStateConnecting => "Connecting...",
						// 	TdApi.ConnectionState.ConnectionStateWaitingForNetwork => "Waiting for network...",
						// 	TdApi.ConnectionState.ConnectionStateConnectingToProxy => "Connecting to proxy...",
						// 	_ => Config.AppName
						// };
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

		private async void ContentDialogNewVersion_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			//NavigationView.FooterMenuItems.RemoveAt(NavigationView.FooterMenuItems.Count - 1);
			
			_updateFilePath = await _updateManager.UpdateApplicationAsync();
			
			NavigationViewItem item = new();
			item.Content = "Install update";
			item.Tag = "InstallUpdate";
			item.Icon = new SymbolIcon(Symbol.Refresh);
			
			//NavigationView.FooterMenuItems.Add(item);
		}

		private void ContentDialogNewVersion_OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (_updateAvailable) return;
			
			NavigationViewItem item = new();
			item.Content = "New update available!";
			item.Tag = "NewUpdate";
			item.Icon = new SymbolIcon(Symbol.Refresh);
			
			//NavigationView.FooterMenuItems.Add(item);
			
			_updateAvailable = true;
		}
	}
}
