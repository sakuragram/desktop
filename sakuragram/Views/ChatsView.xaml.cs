using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using sakuragram.Services.Core;
using sakuragram.Views.Chats;
using TdLib;

namespace sakuragram.Views;

public sealed partial class ChatsView : Page
{
    public TdClient _client = App._client;
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
    private bool _isArchiveState = true;
    private StorageFile _newProfilePicture;
    private List<long> _chatsIds = [];
    private List<long> _pinnedChats = [];
    private List<ChatEntry> _chats = [];

    public MainWindow MainWindow;
    public Frame MainWindowFrame;
    public TitleBar MainWindowTitleBar;
    public StackPanel MainWindowTitleBarContent;
    
    public ItemsControl PinnedChatsItemsList;
    public ItemsControl ChatsItemsList;
        
/// <summary>
/// Initializes a new instance of the <see cref="ChatsView"/> class.
/// </summary>
/// <remarks>
/// This constructor initializes the components for the chat view, sets up the UI elements for displaying pinned
/// and regular chats, and configures event handlers for various UI interactions, such as changing themes and
/// toggling the archive state. It also sets the initial width of the forum topics column.
/// </remarks>
    public ChatsView()
    {
        InitializeComponent();

        var settings = SettingsService.LoadSettings();
        
        PinnedChatsItemsList = PinnedChatsList;
        ChatsItemsList = ChatsList;
            
        ColumnForumTopics.Width = new GridLength(0);

        FlyoutItemExpandArchive.Click += (_, _) => UpdateArchiveState();
        FlyoutItemTelegramFeatures.Click += (_, _) => OpenChat(-1001224624669);
        FlyoutItemSakuragramNews.Click += (_, _) => OpenChat(-1002187436725);
    }

    /// <summary>
    /// Process updates from TDLib.
    /// </summary>
    /// <param name="update">The update to process.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method is called whenever TDLib sends an update to the chat list.
    /// The update is processed here, and the UI is updated accordingly.
    /// </remarks>
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

    public async Task PrepareChatFolders()
    {
    }

    public async void OpenChat(long chatId = 0, bool isForum = false, TdApi.ForumTopic forumTopic = null)
    {
        try
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                if (isForum)
                {
                    _currentChat = await _chatService.OpenForum(chatId, forumTopic);
                    _isForumTopicOpened = true;
                }
                else
                {
                    _currentChat = await _chatService.OpenChat(chatId, XamlRoot);
                    if (_isForumOpened && _isForumTopicOpened) ColumnForumTopics.Width = new GridLength(0);
                    _isForumOpened = false;
                    _isForumTopicOpened = false;
                    _isChatOpened = true;
                }

                _currentChat._ChatsView = this;
                GridChatView.Children.Add(_currentChat);
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public async Task OpenForum(TdApi.Supergroup supergroup, TdApi.Chat chat)
    {
        // TODO: fix
        // _isForumOpened = true;
        // var forumTopics = await _client.ExecuteAsync(new TdApi.GetForumTopics
        // {
        //     ChatId = chat.Id,
        //     Limit = 100,
        //     OffsetMessageId = chat.LastMessage.Id,
        //     OffsetMessageThreadId = chat.LastMessage.MessageThreadId
        // });
        //     
        // await DispatcherQueue.EnqueueAsync(async () =>
        // {
        //     await ChatPhoto.InitializeProfilePhoto(null, chat);
        //     ChatTitle.Text = chat.Title;
        //     ChatMembers.Text = $"{supergroup.MemberCount} members";
        //         
        //     foreach (var forumTopic in forumTopics.Topics)
        //     {
        //         Button button = new(); 
        //         button.Height = 50;
        //         button.Width = 180;
        //         button.Margin = new Thickness(4, 4, 4, 0);
        //         button.Click += (_, _) =>
        //         {
        //             try
        //             {
        //                 OpenChat(chat.Id, isForum: true, forumTopic: forumTopic);
        //                 _isForumTopicOpened = true;
        //                 _isForumOpened = false;
        //             }
        //             catch (Exception e)
        //             {
        //                 Debug.WriteLine(e);
        //                 throw;
        //             }
        //         };
        //             
        //         StackPanel stackPanel = new();
        //         stackPanel.Orientation = Orientation.Vertical;
        //         stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
        //         stackPanel.VerticalAlignment = VerticalAlignment.Stretch;
        //             
        //         TextBlock topicNameTextBlock = new();
        //         topicNameTextBlock.Text = forumTopic.Info.Name;
        //         topicNameTextBlock.FontSize = 12;
        //         stackPanel.Children.Add(topicNameTextBlock);
        //             
        //         TextBlock topicLastMessageTextBlock = new();
        //         topicLastMessageTextBlock.Text = UserService.GetSenderName(forumTopic.LastMessage).Result + ": " 
        //             + MessageService.GetTextMessageContent(forumTopic.LastMessage).Result;
        //         topicLastMessageTextBlock.FontSize = 10;
        //         topicLastMessageTextBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"];
        //         stackPanel.Children.Add(topicLastMessageTextBlock);
        //             
        //         button.Content = stackPanel;
        //         PanelForumTopics.Children.Add(button);
        //     }
        // });
        //     
        // ColumnForumTopics.Width = new GridLength(200);
    }

    public void CloseChat()
    {
        if (_currentChat == null) return;
        if (_isForumTopicOpened)
        {
            _isForumOpened = true;
            ColumnForumTopics.Width = new GridLength(200);
        }
        GridChatView.Children.Remove(_currentChat);
    }

    private async Task GenerateChatEntries(TdApi.ChatList chatList, StackPanel pinnedPanel = null, StackPanel chatsPanel = null)
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
                    ChatPage = GridChatView,
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
        MainWindowTitleBar.IsBackButtonVisible = true;
        MainWindowTitleBar.BackButtonClick += async (_, _) =>
        {
            MainWindowTitleBar.IsBackButtonVisible = false;
            MainWindowFrame.Navigate(typeof(ChatsView));
            MainWindowTitleBarContent.Visibility = Visibility.Visible;
        };
        await AccountManager.AddNewAccount(MainWindow, MainWindowFrame);
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
        var localAccountSettings = SettingsService.LoadAccountSettings();
			
        if (localAccountSettings.AccountIds.Count > 1)
        {
            DispatcherQueue.EnqueueAsync(async () =>
            {
                var accountIds = localAccountSettings.AccountIds;
                var accountIndex = localAccountSettings.SelectedAccount;
                int index = -1;
					   
                foreach (var accountId in accountIds)
                {
                    if (accountId == accountIds[accountIndex]) continue;
                    var user = await _client.GetUserAsync(accountId);
                    var accountItem = new MenuFlyoutItem
                    {
                        Icon = new BitmapIcon
                        {
                            UriSource = new Uri(user.ProfilePhoto.Small.Local.Path), 
                            ShowAsMonochrome = false
                        },
                        Text = user.FirstName + " " + user.LastName,
                        Tag = accountId
                    };
                    accountItem.Click += async (_, _) =>
                    {
                        await AccountManager.SwitchAccount(MainWindow, (long)accountItem.Tag);
                    };
                    index++;
                    FlyoutItemCurrentUser.Items.Add(accountItem);
                }
            });
        }
    }

    private Task GenerateFolders()
    {
        SelectorBarFolders.Items.Clear();
        var localAccountSettings = SettingsService.LoadAccountInfoSettings();
        
        SelectorBarItem selectorBarItemAllChats = new();
        selectorBarItemAllChats.Margin = new Thickness(0, 0, 5, 0);
        selectorBarItemAllChats.Text = "All chats";
        selectorBarItemAllChats.PointerPressed += async (_, _) => await GenerateChatEntries(new TdApi.ChatList.ChatListMain());
        SelectorBarFolders.Items.Add(selectorBarItemAllChats);
        
        foreach (var folder in localAccountSettings.ChatFolders)
        {
            SelectorBarItem selectorBarItem = new();
            selectorBarItem.Margin = new Thickness(0, 0, 5, 0);
            selectorBarItem.Text = folder.Title;
            selectorBarItem.Tag = $"{folder.Title}_{folder.Id}";
            selectorBarItem.PointerPressed += async (_, _) =>
            {
                App._folderId = folder.Id;
                await GenerateChatEntries(new TdApi.ChatList.ChatListFolder{ChatFolderId = folder.Id});
                selectorBarItem.IsSelected = true;
            };
            SelectorBarFolders.Items.Add(selectorBarItem);
        }
        
        return Task.CompletedTask;
    }

    public async Task PrepareChatsView()
    {
        int currentIndex = 0;
        string lastArchiveChannels = string.Empty;

        await DispatcherQueue.EnqueueAsync(async () =>
        {
            var currentUser = await _client.GetMeAsync();
            var archive = await _client.GetChatsAsync(new TdApi.ChatList.ChatListArchive(), 100);

            try
            {
                await CurrentUserPicture.InitializeProfilePhoto(currentUser, sizes: 42);
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
            }

            if (archive.ChatIds.Length > 0)
            {
                ButtonArchive.Visibility = Visibility.Visible;
                SeparatorArchive.Visibility = Visibility.Visible;
                int totalUnreadCount = 0;

                foreach (var chatId in archive.ChatIds)
                {
                    var chat = await _client.GetChatAsync(chatId);
                    totalUnreadCount += chat.UnreadCount;

                    if (currentIndex != 3 && _isArchiveState)
                    {
                        lastArchiveChannels += chat.Title + ", ";
                        currentIndex++;
                    }
                }

                BadgeArchiveUnreadCount.Value = totalUnreadCount;
                TextBlockLastChat.Text = lastArchiveChannels;
            }
        });
        
        GenerateUsers();
        await GenerateFolders();
        UpdateArchiveState();
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); }; 
        
        if (App._folderId != -1)
        {
            await GenerateChatEntries(new TdApi.ChatList.ChatListFolder{ChatFolderId = App._folderId});
        }
        else
        { 
            await GenerateChatEntries(new TdApi.ChatList.ChatListMain());
            App._folderId = -1;
        }
    }

    private void UpdateArchiveState()
    {
        var settings = SettingsService.LoadSettings();
        SettingsService.SaveSettings(new Services.Core.Settings
        {
            InstallBeta = settings.InstallBeta,
            ArchiveState = !settings.ArchiveState,
            AutoUpdate = settings.AutoUpdate,
            Language = settings.Language,
            ChatBottomFastAction = settings.ChatBottomFastAction,
            StartMediaPage = settings.StartMediaPage
        });
        _isArchiveState = !_isArchiveState;
        FlyoutItemExpandArchive.Text = _isArchiveState ? "Expand" : "Collapse";
        ButtonArchive.Height = _isArchiveState ? 40 : 50;
        TextBlockLastChat.Visibility = _isArchiveState ? Visibility.Collapsed : Visibility.Visible;
    }
}