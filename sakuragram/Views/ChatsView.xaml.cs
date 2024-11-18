using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using sakuragram.Services.Core;
using sakuragram.Views.Chats;
using TdLib;
using DispatcherQueuePriority = Microsoft.UI.Dispatching.DispatcherQueuePriority;

namespace sakuragram.Views;

public sealed partial class ChatsView : Page
{
    private static TdClient _client = App._client;
    private ChatService _chatService = App.ChatService;
    private TdApi.Chats _addedChats;
        
    public Chat _currentChat;
    private bool _bInArchive = false;
    private bool _firstGenerate = true;
    private bool _createChannelOpened = false;
    private bool _createGroupOpened = false;
    private bool _isForumOpened = false;
    private bool _isForumTopicOpened = false;
    private bool _isChatOpened = false;
    private int _totalUnreadArchivedChatsCount = 0;
    private StorageFile _newProfilePicture;
    private List<long> _chatsIds = [];
    private List<long> _pinnedChats = [];
    private List<ChatEntry> _chats = [];

    public MainWindow MainWindow;
    public Frame MainWindowFrame;
    public TitleBar MainWindowTitleBar;
    public StackPanel MainWindowTitleBarContent;
        
    public ChatsView()
    {
        InitializeComponent();
            
        SelectorBarItem selectorBarItemAllChats = new();
        selectorBarItemAllChats.Margin = new Thickness(0, 0, 5, 0);
        selectorBarItemAllChats.Text = "All chats";
        selectorBarItemAllChats.PointerPressed += (_, _) => GenerateChatEntries(new TdApi.ChatList.ChatListMain());
        SelectorBarFolders.Items.Add(selectorBarItemAllChats);
        
        foreach (var folder in App._folders)
        {
            SelectorBarItem selectorBarItem = new();
            selectorBarItem.Margin = new Thickness(0, 0, 5, 0);
            selectorBarItem.Text = folder.Title;
            selectorBarItem.Tag = $"{folder.Title}_{folder.Id}";
            selectorBarItem.PointerPressed += (_, _) =>
            {
                App._folderId = folder.Id;
                GenerateChatEntries(new TdApi.ChatList.ChatListFolder{ChatFolderId = folder.Id});
                selectorBarItem.IsSelected = true;
            };
            SelectorBarFolders.Items.Add(selectorBarItem);
        }
        
        if (App._folderId != -1)
        {
            GenerateChatEntries(new TdApi.ChatList.ChatListFolder{ChatFolderId = App._folderId});
        }
        else
        {
            GenerateChatEntries(new TdApi.ChatList.ChatListMain());
            App._folderId = -1;
        }
            
        ColumnForumTopics.Width = new GridLength(0);
        
        FlyoutItemTelegramFeatures.Click += (_, _) =>
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("https://t.me/TelegramTips");
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        };
        FlyoutItemSakuragramNews.Click += (_, _) =>
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("https://t.me/sakuragram");
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        };
        
        // GenerateUsers();

        DispatcherQueue.EnqueueAsync(async () =>
        {
            try
            {
                var currentUser = await _client.GetMeAsync();
                await CurrentUserPicture.InitializeProfilePhoto(currentUser, null, 42, 42);
                FlyoutItemCurrentUser.Text = currentUser.FirstName + " " + currentUser.LastName;
                FlyoutItemCurrentUser.Icon = new BitmapIcon
                {
                    ShowAsMonochrome = false,
                    UriSource = new Uri(currentUser.ProfilePhoto.Small.Local.Path),
                    Width = 32,
                    Height = 32
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
        
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); }; 
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateNewMessage updateNewMessage:
            {
                await DispatcherQueue.EnqueueAsync(() => UpdateChatPosition(updateNewMessage.Message.ChatId, ChatsList, ChatsList));
                break;
            }
            case TdApi.Update.UpdateChatPosition updateChatPosition:
            {
                if (updateChatPosition.Position.IsPinned)
                {
                    await DispatcherQueue.EnqueueAsync(() => UpdateChatPosition(updateChatPosition.ChatId, ChatsList, PinnedChatsList));
                }
                else
                {
                    await DispatcherQueue.EnqueueAsync(() => UpdateChatPosition(updateChatPosition.ChatId, PinnedChatsList, ChatsList));
                }
                break;
            }
            case TdApi.Update.UpdateChatDraftMessage updateChatDraftMessage:
            {
                await DispatcherQueue.EnqueueAsync(() => UpdateChatPosition(updateChatDraftMessage.ChatId, ChatsList, ChatsList));
                break;
            }
        }
    }

    private void UpdateChatPosition(long chatId, ItemsControl chatsListToRemove, ItemsControl chatsListToInsert)
    {
        DispatcherQueue.EnqueueAsync(() =>
        {
            var chatToMove = ChatsList.Items
                .OfType<ChatEntry>()
                .FirstOrDefault(chat => chat.ChatId == chatId);
            if (chatToMove == null || chatToMove.ChatId != chatId) return;
            chatsListToRemove.Items.Remove(chatToMove);
            chatsListToInsert.Items.Insert(0, chatToMove);
        });
    }
        
    public async void OpenChat(long chatId, bool isForum, TdApi.ForumTopic forumTopic)
    {
        try
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (isForum)
                {
                    _currentChat = _chatService.OpenForum(chatId, forumTopic).Result;
                    _isForumTopicOpened = true;
                }
                else
                {
                    _currentChat = _chatService.OpenChat(chatId).Result;
                    if (_isForumOpened && _isForumTopicOpened) ColumnForumTopics.Width = new GridLength(0);
                    _isForumOpened = false;
                    _isForumTopicOpened = false;
                    _isChatOpened = true;
                }

                _currentChat._ChatsView = this;
                ContentFrame.Children.Add(_currentChat);
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async void OpenForum(TdApi.Supergroup supergroup, TdApi.Chat chat)
    {
        _isForumOpened = true;
        var forumTopics = await _client.ExecuteAsync(new TdApi.GetForumTopics
        {
            ChatId = chat.Id,
            Limit = 100,
            OffsetMessageId = chat.LastMessage.Id,
            OffsetMessageThreadId = chat.LastMessage.MessageThreadId
        });
            
        await DispatcherQueue.EnqueueAsync(async () =>
        {
            await ChatPhoto.InitializeProfilePhoto(null, chat);
            ChatTitle.Text = chat.Title;
            ChatMembers.Text = $"{supergroup.MemberCount} members";
                
            foreach (var forumTopic in forumTopics.Topics)
            {
                Button button = new(); 
                button.Height = 50;
                button.Width = 180;
                button.Margin = new Thickness(4, 4, 4, 0);
                button.Click += (_, _) =>
                {
                    try
                    {
                        OpenChat(chat.Id, true, forumTopic);
                        _isForumTopicOpened = true;
                        _isForumOpened = false;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        throw;
                    }
                };
                    
                StackPanel stackPanel = new();
                stackPanel.Orientation = Orientation.Vertical;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                stackPanel.VerticalAlignment = VerticalAlignment.Stretch;
                    
                TextBlock topicNameTextBlock = new();
                topicNameTextBlock.Text = forumTopic.Info.Name;
                topicNameTextBlock.FontSize = 12;
                stackPanel.Children.Add(topicNameTextBlock);
                    
                TextBlock topicLastMessageTextBlock = new();
                topicLastMessageTextBlock.Text = UserService.GetSenderName(forumTopic.LastMessage).Result + ": " 
                    + MessageService.GetTextMessageContent(forumTopic.LastMessage).Result;
                topicLastMessageTextBlock.FontSize = 10;
                topicLastMessageTextBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"];
                stackPanel.Children.Add(topicLastMessageTextBlock);
                    
                button.Content = stackPanel;
                PanelForumTopics.Children.Add(button);
            }
        });
            
        ColumnForumTopics.Width = new GridLength(200);
    }

    public void CloseChat()
    {
        if (_currentChat == null) return;
        if (_isForumTopicOpened)
        {
            _isForumOpened = true;
            ColumnForumTopics.Width = new GridLength(200);
        }
        ContentFrame.Children.Remove(_currentChat);
    }
        
    private async Task GenerateChatEntries(TdApi.ChatList chatList)
    {
        PinnedChatsList.Items.Clear();
        ChatsList.Items.Clear();
        _pinnedChats.Clear();
        _chatsIds.Clear();
        _chats.Clear();
            
        _addedChats = await _client.GetChatsAsync(chatList, 10000);
                     
        foreach (var chatId in _addedChats.ChatIds)
        {
            var chat = await _client.GetChatAsync(chatId);

            foreach (var chatPosition in chat.Positions)
            {
                if (!chatPosition.IsPinned) continue;
                if (_pinnedChats.Contains(chat.Id)) continue;

                if (chatPosition.List is TdApi.ChatList.ChatListFolder folder && folder.ChatFolderId != App._folderId)
                {
                    continue;
                }
                
                switch (chatPosition.List)
                {
                    case TdApi.ChatList.ChatListMain:
                    {
                        _pinnedChats.Add(chat.Id);
                        break;
                    }
                    case TdApi.ChatList.ChatListFolder chatListFolder:
                    {
                        if (App._folderId != -1)
                        {
                            chatListFolder.ChatFolderId = App._folderId;
                            _pinnedChats.Add(chat.Id);
                        }
                        break;
                    }
                }
            }

            if (_pinnedChats.Count == 0)
                await DispatcherQueue.EnqueueAsync(() => Separator.Visibility = Visibility.Collapsed);
            else await DispatcherQueue.EnqueueAsync(() => Separator.Visibility = Visibility.Visible);

            await DispatcherQueue.EnqueueAsync(() =>
            {
                ChatEntry chatEntry = new()
                {
                    _ChatsView = this,
                    ChatPage = ContentFrame,
                    _chat = chat,
                    ChatId = chat.Id,
                };
                
                _chatsIds.Add(chat.Id);
                _chats.Add(chatEntry);

                if (_pinnedChats.Contains(chat.Id))
                { 
                    PinnedChatsList.Items.Add(chatEntry);
                }
                else
                { 
                    ChatsList.Items.Add(chatEntry);
                }
            });
        }   
        
        _firstGenerate = false;
    }

    private void ButtonSavedMessages_OnClick(object sender, RoutedEventArgs e)
    {
        OpenChat(_client.ExecuteAsync(new TdApi.GetMe()).Result.Id, false, null);
    }

    private void ButtonNewMessage_OnClick(object sender, RoutedEventArgs e)
    {
        NewMessage.ShowAsync();
    }

    private async void NewGroup_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            var newGroup = await _client.ExecuteAsync(new TdApi.CreateNewBasicGroupChat
            {
                Title = TextBoxGroupName.Text,
                UserIds = null,
            }).ConfigureAwait(false);

            await _client.ExecuteAsync(new TdApi.SetChatPhoto
            {
                ChatId = newGroup.ChatId,
                Photo = new TdApi.InputChatPhoto.InputChatPhotoStatic
                {
                    Photo = new TdApi.InputFile.InputFileLocal
                    {
                        Path = _newProfilePicture.Path
                    }
                }
            });
        }
        catch (TdException e)
        {
            TextBlockGroupCreateException.Text = e.Message;
            TextBlockGroupCreateException.Visibility = Visibility.Visible;
            throw;
        }
    }
        
    private async void NewChannel_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            var newChannel = await _client.ExecuteAsync(new TdApi.CreateNewSupergroupChat
            {
                IsChannel = true,
                Title = TextBoxChannelName.Text,
                Description = TextBoxChannelDescription.Text
            }).ConfigureAwait(false);
                
            await _client.ExecuteAsync(new TdApi.SetChatPhoto
            {
                ChatId = newChannel.Id, 
                Photo = new TdApi.InputChatPhoto.InputChatPhotoStatic
                {
                    Photo = new TdApi.InputFile.InputFileLocal 
                    {
                        Path = _newProfilePicture.Path
                    }
                }
            });
        }
        catch (TdException e)
        {
            TextBlockChannelCreateException.Text = e.Message;
            TextBlockChannelCreateException.Visibility = Visibility.Visible;
            throw;
        }
    }
        
    private void CreateNewGroup_OnClick(object sender, RoutedEventArgs e)
    {
        NewMessage.Hide();
        NewGroup.ShowAsync();
    }

    private void CreateNewChannel_OnClick(object sender, RoutedEventArgs e)
    {
        NewMessage.Hide();
        NewChannel.ShowAsync();
    }

    private void ChatsView_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Escape:
                if (_isForumOpened) ColumnForumTopics.Width = new GridLength(0);
                else if (_isForumTopicOpened || _isChatOpened) CloseChat();
                break;
        }
    }

    private async void ButtonUploadGroupPhoto_OnClick(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FileOpenPicker();

        var mainWindow = (Application.Current as App)?._mWindow;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        folderPicker.FileTypeFilter.Add(".png");
        folderPicker.FileTypeFilter.Add(".jpg");
        folderPicker.FileTypeFilter.Add(".jpeg");
        folderPicker.FileTypeFilter.Add(".webm");
        folderPicker.FileTypeFilter.Add(".webp");
        var newPhotoFile = await folderPicker.PickSingleFileAsync();
        
        if (newPhotoFile != null)
        {
            _newProfilePicture = newPhotoFile;
            if (_createChannelOpened)
            {
                NewChannelPicture.ProfilePicture = new BitmapImage(new Uri(_newProfilePicture.Path));
            }

            if (_createGroupOpened)
            {
                NewGroupPicture.ProfilePicture = new BitmapImage(new Uri(_newProfilePicture.Path));
            }
        }
    }

    private void NewChannel_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        _createChannelOpened = true;
    }

    private void NewChannel_OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _createChannelOpened = false;
    }

    private void NewGroup_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        _createGroupOpened = true;
    }

    private void NewGroup_OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _createGroupOpened = false;
    }

    private void ScrollViewer_OnViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;
        if (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight >= scrollViewer.ScrollableHeight)
        {
            for (int i = 0; i < 10; i++) ;
        }
    }
    
    private void FlyoutClientActions_OnOpening(object sender, object e)
    {
			
    }

    private void FlyoutClientActions_OnClosing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
    {
			
    }
    
    private async void FlyoutItemAddAccount_OnClick(object sender, RoutedEventArgs e)
    {
        MainWindowTitleBarContent.Visibility = Visibility.Collapsed;
        await App.AddNewAccount();
        MainWindowTitleBar.IsBackButtonVisible = true;
        MainWindowTitleBar.BackButtonClick += async (_, _) =>
        {
            await App.RemoveNewAccount();
            MainWindowTitleBar.IsBackButtonVisible = false;
            MainWindowFrame.Navigate(typeof(ChatsView));
            MainWindowTitleBarContent.Visibility = Visibility.Visible;
        };
        MainWindowFrame.Navigate(typeof(LoginView));
        if (MainWindowFrame.Content is LoginView login) login._window = MainWindow;
    }

    private async void FlyoutItemLogOut_OnClick(object sender, RoutedEventArgs e)
    {
        await _client.LogOutAsync();
        await _client.CloseAsync();
        await _client.DestroyAsync();
        MainWindowFrame.Navigate(typeof(LoginView));
    }

    private void FlyoutItemSavedMessages_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void FlyoutItemSettings_OnClick(object sender, RoutedEventArgs e)
    {
        MainWindowFrame.Navigate(typeof(SettingsView));
			     
        MainWindowTitleBar.IsBackButtonVisible = true;
        MainWindowTitleBar.BackButtonClick += (_, _) =>
        {
            MainWindowTitleBar.IsBackButtonVisible = false;
            MainWindowFrame.Navigate(typeof(ChatsView));
        };
    }

    private void FlyoutItemCurrentUser_OnClick(object sender, RoutedEventArgs e)
    {
        MainWindowFrame.Navigate(typeof(SettingsView));
    }
    
    private void CurrentUserPicture_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        FlyoutClientActions.ShowAt(CurrentUserPicture, new FlyoutShowOptions 
            { ShowMode = FlyoutShowMode.Transient, Placement = FlyoutPlacementMode.Top });
    }
    
    private void GenerateUsers()
    {
        var localSettings = SettingsService.LoadSettings();
			
        if (localSettings.ClientIDs.Count > 1)
        {
            DispatcherQueue.EnqueueAsync(async () =>
            {
                var clientIds = localSettings.ClientIDs;
                var clientIndex = localSettings.ClientIndex;
                int index = 0;
					
                foreach (var clientId in localSettings.ClientIDs)
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
                        MainWindowFrame.Navigate(typeof(ChatsView));
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