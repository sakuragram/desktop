using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Octokit;
using sakuragram.Services.Core;
using sakuragram.Views;
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
		private List<PersonPicture> _stories = [];
		
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

            DispatcherQueue.EnqueueAsync(async () =>
            {
	            try
	            {
		            var currentUser = await _client.GetMeAsync();
		            if (currentUser.ProfilePhoto != null)
		            {
			            if (currentUser.ProfilePhoto.Small.Local.IsDownloadingCompleted)
			            {
				            CurrentUserPicture.ProfilePicture = new BitmapImage(new Uri(currentUser.ProfilePhoto.Small.Local.Path));
			            }
			            else
			            {
				            var file = await _client.ExecuteAsync(new TdApi.DownloadFile
				            {
					            FileId = currentUser.ProfilePhoto.Small.Id,
					            Priority = 1
				            });

				            if (file.Local.IsDownloadingCompleted) CurrentUserPicture.ProfilePicture = new BitmapImage(new Uri(file.Local.Path));
			            }
		            }
		            else
		            {
			            CurrentUserPicture.Initials = currentUser.FirstName + currentUser.LastName;
		            }
	            }
	            catch (TdException e)
	            {
		            Console.WriteLine(e);
		            throw;
	            }
            });
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

		private void CurrentUserPicture_OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			FlyoutClientActions.ShowAt(CurrentUserPicture, new FlyoutShowOptions 
				{ ShowMode = FlyoutShowMode.Transient, Placement = FlyoutPlacementMode.Bottom });
		}

		private void FlyoutItemAddAccount_OnClick(object sender, RoutedEventArgs e)
		{
		}

		private async void FlyoutItemLogOut_OnClick(object sender, RoutedEventArgs e)
		{
			await _client.LogOutAsync();
			await _client.CloseAsync();
			var loginView = new LoginView();
			loginView.Activate();
			Close();
		}

		private void FlyoutItemSavedMessages_OnClick(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void FlyoutItemSettings_OnClick(object sender, RoutedEventArgs e)
		{
			var settingsView = Assembly.GetExecutingAssembly().GetType($"{Config.AppName}.Views.SettingsView");
			ContentFrame.Navigate(settingsView);
			
			TitleBar.IsBackButtonVisible = true;
			TitleBar.BackButtonClick += (_, _) =>
			{
				TitleBar.IsBackButtonVisible = false;
				var chatsView = Assembly.GetExecutingAssembly().GetType($"{Config.AppName}.Views.ChatsView");
				ContentFrame.Navigate(chatsView);
			};
		}
	}
}
