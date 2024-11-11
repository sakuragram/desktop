using System;
using System.Collections.Generic;
using System.Linq;
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
using sakuragram.Services;
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
		public static Settings _localSettings = SettingsService.LoadSettings();
		
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
		            await MediaService.GetUserPhoto(currentUser, CurrentUserPicture);
		            FlyoutItemCurrentUser.Text = currentUser.FirstName + " " + currentUser.LastName;
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

		private void FlyoutItemCurrentUser_OnClick(object sender, RoutedEventArgs e)
		{
			var settingsView = Assembly.GetExecutingAssembly().GetType($"{Config.AppName}.Views.SettingsView");
			ContentFrame.Navigate(settingsView);
		}
		
		private async void SuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				int maxLength = 40;
				var chats = await _client.SearchChatsAsync(SuggestBox.Text, 100);
				var messages = await _client.SearchMessagesAsync(new TdApi.ChatList.ChatListMain(),
					false, SuggestBox.Text, limit: 100, 
					filter: new TdApi.SearchMessagesFilter.SearchMessagesFilterEmpty());
				
				var suitableItems = new List<string>();
				var splitText = sender.Text.ToLower().Split(" ");
				
				foreach (var chatId in chats.ChatIds)
				{
					var chat = await _client.GetChatAsync(chatId);
					var found = splitText.All(key=> chat.Title.ToLower().Contains(key));
					if (found)
					{
						suitableItems.Add(TextService.Truncate(chat.Title, maxLength));
					}
				}
				
				foreach (var message in messages.Messages)
				{
					var messageContent = await MessageService.GetLastMessageContent(message);
					var found = splitText.All(key => messageContent.ToLower().Contains(key));
					if (found)
					{
						var messageSender = await UserService.GetSender(message.SenderId);
						var messageSenderName = await UserService.GetSenderName(message);
						
						if (messageSender.User != null)
						{
							suitableItems.Add(TextService.Truncate($"{messageSenderName}: {messageContent}",
								maxLength));
						}
						else if (messageSender.Chat != null)
						{
							switch (messageSender.Chat.Type)
							{
								case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
								{
									if (typeSupergroup.IsChannel)
									{
										suitableItems.Add(TextService.Truncate($"{messageSender.Chat.Title}: {messageContent}",
											maxLength));
									}
									else
									{
										suitableItems.Add(TextService.Truncate($"{messageSenderName}: {messageContent}",
											maxLength));
									}
									break;
								}
								case TdApi.ChatType.ChatTypeBasicGroup:
									suitableItems.Add(TextService.Truncate($"{messageSenderName}: {messageContent}",
										maxLength));
									break;
							}
						}
					}
				}
				
				if(suitableItems.Count == 0)
				{
					suitableItems.Add("No results found");
				}
				sender.ItemsSource = suitableItems;
			}
		}

		private void SuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			SuggestBox.Text = args.SelectedItem.ToString();
		}
	}
}
