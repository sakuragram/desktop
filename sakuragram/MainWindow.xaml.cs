using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Octokit;
using sakuragram.Controls.User;
using sakuragram.Services;
using sakuragram.Services.Core;
using sakuragram.Views;
using TdLib;
using DispatcherQueuePriority = Microsoft.UI.Dispatching.DispatcherQueuePriority;

namespace sakuragram;

public sealed partial class MainWindow : Window
{
	public Frame RootFrame;
	public StackPanel TopBarContent;
		
	private static TdClient _client = App._client;
	private static TdApi.User _user;
	private static TdApi.ChatFolderInfo[] _folders = App._folders;

	private NavigationViewItem _lastItem;
	private int _totalUnreadCount;
	private string _updateFilePath;
	private bool _updateAvailable;
	private List<TdApi.ChatActiveStories> _stories = [];
		
	private UpdateManager _updateManager = App.UpdateManager;
	private GitHubClient _gitHubClient = App._githubClient;
	public static Settings _localSettings = SettingsService.LoadSettings();
		
	public MainWindow()
	{
		InitializeComponent();
			
		RootFrame = ContentFrame;
		TopBarContent = PanelContent;

		#region TitleBar

		TitleBar.Title = Config.AppName;
		TitleBar.Icon = new BitmapIcon 
		{
			UriSource = new Uri("ms-appx:///Assets/StoreLogo.scale-400.png"), 
			ShowAsMonochrome = false
		};
		ExtendsContentIntoTitleBar = true;
		AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;

		#endregion

		#region App title

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
		#endregion
			
		TrySetDesktopAcrylicBackdrop();
            
		if (App._authNeeded)
		{
			PanelContent.Visibility = Visibility.Collapsed;
			ContentFrame.Navigate(typeof(LoginView));
			if (ContentFrame.Content is LoginView login) login._window = this;
		}
		else
		{
			ContentFrame.Navigate(typeof(ChatsView));
	            
			GenerateUsers();
			GenerateStories();
			CheckForUpdates();
	            
			DispatcherQueue.EnqueueAsync(async () =>
			{
				try
				{
					var currentUser = await _client.GetMeAsync();
					await MediaService.GetUserPhoto(currentUser, CurrentUserPicture);
					FlyoutItemCurrentUser.Text = currentUser.FirstName + " " + currentUser.LastName;
					foreach (var story in _stories)
					{
						if (story.Order == 0) continue;
						var storyElement = new Story(story);
						PanelStories.Children.Add(storyElement);
					}
				}
				catch (TdException e)
				{
					Console.WriteLine(e);
					throw;
				}
			});
			
			_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
			NotificationService notificationService = new();
		}
	}

	private static async void GenerateStories()
	{
		await _client.LoadActiveStoriesAsync(new TdApi.StoryList.StoryListMain());
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
			case TdApi.Update.UpdateChatActiveStories updateChatActiveStories:
			{
				_stories.Add(updateChatActiveStories.ActiveStories);
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

	private async void FlyoutItemAddAccount_OnClick(object sender, RoutedEventArgs e)
	{
		PanelContent.Visibility = Visibility.Collapsed;
		await App.AddNewAccount();
		TitleBar.IsBackButtonVisible = true;
		TitleBar.BackButtonClick += async (_, _) =>
		{
			await App.RemoveNewAccount();
			TitleBar.IsBackButtonVisible = false;
			ContentFrame.Navigate(typeof(ChatsView));
			PanelContent.Visibility = Visibility.Visible;
		};
		ContentFrame.Navigate(typeof(LoginView));
		if (ContentFrame.Content is LoginView login) login._window = this;
	}

	private async void FlyoutItemLogOut_OnClick(object sender, RoutedEventArgs e)
	{
		await _client.LogOutAsync();
		await _client.CloseAsync();
		await _client.DestroyAsync();
		ContentFrame.Navigate(typeof(LoginView));
	}

	private void FlyoutItemSavedMessages_OnClick(object sender, RoutedEventArgs e)
	{
	}

	private void FlyoutItemSettings_OnClick(object sender, RoutedEventArgs e)
	{
		ContentFrame.Navigate(typeof(SettingsView));
			
		TitleBar.IsBackButtonVisible = true;
		TitleBar.BackButtonClick += (_, _) =>
		{
			TitleBar.IsBackButtonVisible = false;
			ContentFrame.Navigate(typeof(ChatsView));
		};
	}

	private void FlyoutItemCurrentUser_OnClick(object sender, RoutedEventArgs e)
	{
		ContentFrame.Navigate(typeof(SettingsView));
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
				var messageContent = await MessageService.GetTextMessageContent(message);
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

	private void FlyoutClientActions_OnOpening(object sender, object e)
	{
			
	}

	private void FlyoutClientActions_OnClosing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
	{
			
	}

	private void GenerateUsers()
	{
		_localSettings = SettingsService.LoadSettings();
			
		if (_localSettings.ClientIDs.Count > 1)
		{
			DispatcherQueue.EnqueueAsync(async () =>
			{
				var clientIds = _localSettings.ClientIDs;
				var clientIndex = _localSettings.ClientIndex;
				int index = 0;
					
				foreach (var clientId in _localSettings.ClientIDs)
				{
					var user = await _client.GetUserAsync(clientId);
					var accountItem = new MenuFlyoutItem();
					accountItem.Text = user.FirstName + " " + user.LastName;
					accountItem.Tag = clientIds.IndexOf(clientId) == clientIndex
						? clientIds.IndexOf(clientId)
						: "error";
					accountItem.Click += async (_, _) =>
					{
						await App.SwitchAccount(clientIds.IndexOf(index));
						ContentFrame.Navigate(typeof(ChatsView));
					};
					index++;
					FlyoutClientActions.Items.Add(accountItem);
				}
			});
		}
		else
		{
			SeparatorUsers.Visibility = Visibility.Collapsed;
		}
	}
}