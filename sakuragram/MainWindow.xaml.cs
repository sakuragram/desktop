using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
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

	private NavigationViewItem _lastItem;
	private int _totalUnreadCount;
	private string _updateFilePath;
	private bool _updateAvailable;
	private List<TdApi.ChatActiveStories> _stories = [];
		
	private UpdateManager _updateManager = App.UpdateManager;
	private GitHubClient _gitHubClient = App._githubClient;
	public static Settings _localSettings = SettingsService.LoadSettings();

	private TdApi.ScopeNotificationSettings _mutedScopeNotificationSettings;
	private TdApi.ScopeNotificationSettings _unmutedScopeNotificationSettings;
	private bool _isAllMuted;
	private bool _isChannelsMuted;
	private bool _isGroupsMuted;
	private bool _isPrivateChatsMuted;
		
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
// #if DEBUG
// 		{
// 			Title = $"{Config.AppName} debug";
// 			TitleBar.Subtitle = "debug";
// 		}
// #elif BETA
// 			{
// 				Title = Config.AppName + " beta";
// 				TitleBar.Subtitle = $"beta ({Config.AppVersion}, TdLib {Config.TdLibVersion})";
// 			}
// #elif RELEASE
// 			{
// 				Title = Config.AppName;
// 				TitleBar.Subtitle = Config.AppVersion;
// 			}
// #else
// 			{
// 				Title = Config.AppName;
// 				TitleBar.Subtitle = "UNTITLED CONFIGURATION";
// 			}
// #endif
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
			OpenChatsView();
			
			CheckForUpdates();
	            
			DispatcherQueue.EnqueueAsync(async () =>
			{
				await _client.LoadActiveStoriesAsync(new TdApi.StoryList.StoryListMain());
				var user = await _client.GetMeAsync();
				
				Title = user.FirstName + " " + user.LastName + " (" + _totalUnreadCount + ")";
				
				foreach (var story in _stories)
				{
					if (story.Order == 0) continue;
					var storyElement = new Story(story);
					PanelStories.Children.Add(storyElement);
				}
				
				FlyoutItemMuteFor0.Click += (_, _) => MuteChatsFor(2);
				FlyoutItemMuteFor1.Click += (_, _) => MuteChatsFor(8);
				FlyoutItemMuteFor2.Click += (_, _) => MuteChatsFor(24);
				
				await UpdateNotificationInfo();
			});
			_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
			NotificationService notificationService = new();

			_mutedScopeNotificationSettings = new TdApi.ScopeNotificationSettings { MuteFor = int.MaxValue };
			_unmutedScopeNotificationSettings = new TdApi.ScopeNotificationSettings { MuteFor = 0 };
		}
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
			case TdApi.Update.UpdateNewMessage:
			{
				_totalUnreadCount += 1;
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

	private async void FlyoutItemMuteAll_OnClick(object sender, RoutedEventArgs e)
	{
		if (!_isAllMuted)
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _mutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeChannelChats()
			});
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _mutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeGroupChats()
			});
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _mutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopePrivateChats()
			});
		}
		else
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _unmutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeChannelChats()
			});
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _unmutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeGroupChats()
			});
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _unmutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopePrivateChats()
			});
		}

		await UpdateNotificationInfo();
	}

	private async void FlyoutItemMuteChannels_OnClick(object sender, RoutedEventArgs e)
	{
		if (!_isChannelsMuted)
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _mutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeChannelChats()
			});
		}
		else
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _unmutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeChannelChats()
			});
		}

		await UpdateNotificationInfo();
	}

	private async void FlyoutItemMuteGroups_OnClick(object sender, RoutedEventArgs e)
	{
		if (!_isGroupsMuted)
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _mutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeGroupChats()
			});
		}
		else
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _unmutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeGroupChats()
			});
		}

		await UpdateNotificationInfo();
	}

	private async void FlyoutItemMutePc_OnClick(object sender, RoutedEventArgs e)
	{
		if (!_isPrivateChatsMuted)
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _mutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopePrivateChats()
			});
		}
		else
		{
			await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
			{
				NotificationSettings = _unmutedScopeNotificationSettings,
				Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopePrivateChats()
			});
		}

		await UpdateNotificationInfo();
	}

	private async Task UpdateNotificationInfo()
	{
		var channelNotificationSettings = await _client.GetScopeNotificationSettingsAsync(
			new TdApi.NotificationSettingsScope.NotificationSettingsScopeChannelChats());
		var groupsNotificationSettings = await _client.GetScopeNotificationSettingsAsync(
			new TdApi.NotificationSettingsScope.NotificationSettingsScopeGroupChats());
		var privateNotificationSettings = await _client.GetScopeNotificationSettingsAsync(
			new TdApi.NotificationSettingsScope.NotificationSettingsScopePrivateChats());
		
		_isChannelsMuted = channelNotificationSettings.MuteFor >= 4000000;
		_isPrivateChatsMuted = privateNotificationSettings.MuteFor >= 4000000;
		_isGroupsMuted = groupsNotificationSettings.MuteFor >= 4000000;
		_isAllMuted = _isChannelsMuted && _isGroupsMuted && _isPrivateChatsMuted;

		await DispatcherQueue.EnqueueAsync(() =>
		{
			FlyoutItemMuteChannels.Text = _isChannelsMuted ? "Unmute channels" : "Mute channels";
			FlyoutItemMuteGroups.Text = _isGroupsMuted ? "Unmute groups" : "Mute groups";
			FlyoutItemMutePc.Text = _isPrivateChatsMuted ? "Unmute private chats" : "Mute private chats";
			FlyoutItemMuteAll.Text = _isAllMuted ? "Unmute all" : "Mute all";
			FlyoutItemMuteChats.Text = _isAllMuted ? "Unmute chats" : "Mute chats";
			
			IconMute.Glyph = _isAllMuted ? "\uE74F" : "\uE767";
			SubIconMute.Glyph = _isAllMuted ? "\uE74F" : "\uE767";
		});
	}

	private async void MuteChatsFor(int muteFor)
	{
		var mutedForHours = new TdApi.ScopeNotificationSettings { MuteFor = muteFor * 60 * 60 };

		await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
		{
			NotificationSettings = mutedForHours,
			Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeChannelChats()
		});
		await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
		{
			NotificationSettings = mutedForHours,
			Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopeGroupChats()
		});
		await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
		{
			NotificationSettings = mutedForHours,
			Scope = new TdApi.NotificationSettingsScope.NotificationSettingsScopePrivateChats()
		});
	}

	public void OpenChatsView()
	{
		ContentFrame.Navigate(typeof(ChatsView), null, 
			new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
		
		if (ContentFrame.Content is ChatsView chatsView)
		{
			chatsView.MainWindow = this;
			chatsView.MainWindowFrame = ContentFrame;
			chatsView.MainWindowTitleBar = TitleBar;
			chatsView.MainWindowTitleBarContent = TopBarContent;
		}
	}
}