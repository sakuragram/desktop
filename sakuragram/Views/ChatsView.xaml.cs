using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Views.Chats;
using TdLib;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using DispatcherQueuePriority = Microsoft.UI.Dispatching.DispatcherQueuePriority;

namespace sakuragram.Views
{
    public sealed partial class ChatsView : Page
    {
        private static TdClient _client = App._client;
        private TdApi.Chats _addedChats;
        
        public Chat _currentChat;
        private bool _bInArchive = false;
        private bool _firstGenerate = true;
        private bool _createChannelOpened = false;
        private bool _createGroupOpened = false;
        private int _totalUnreadArchivedChatsCount = 0;
        private StorageFile _newProfilePicture;
        private List<long> _chatsIds = [];
        private List<long> _pinnedChats = [];
        private List<Button> _foldersButtons = [];
        private List<ChatEntry> _chats = [];
        
        public ChatsView()
        {
            InitializeComponent();
            UpdateArchivedChatsCount();
            
            Button buttonAllChats = new();
            buttonAllChats.Margin = new Thickness(0, 0, 5, 0);
            buttonAllChats.Content = "All chats";
            buttonAllChats.Click += (_, _) => GenerateChatEntries(new TdApi.ChatList.ChatListMain());
            _foldersButtons.Add(buttonAllChats);
            
            foreach (var folder in App._folders)
            {
                Button button = new();
                button.Margin = new Thickness(0, 0, 5, 0);
                button.Content = folder.Title;
                button.Tag = $"{folder.Title}_{folder.Id}";
                button.Click += (_, _) =>
                {
                    App._folderId = folder.Id;
                    GenerateChatEntries(new TdApi.ChatList.ChatListFolder{ChatFolderId = folder.Id});
                };
                _foldersButtons.Add(button);
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
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); }; 
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewMessage updateNewMessage:
                {
                    UpdateChatPosition(updateNewMessage.Message.ChatId, ChatsList, ChatsList);
                    break;
                }
                case TdApi.Update.UpdateChatPosition updateChatPosition:
                {
                    if (updateChatPosition.Position.IsPinned)
                    {
                        UpdateChatPosition(updateChatPosition.ChatId, ChatsList, PinnedChatsList);
                    }
                    else
                    {
                        UpdateChatPosition(updateChatPosition.ChatId, PinnedChatsList, ChatsList);
                    }
                    break;
                }
                case TdApi.Update.UpdateNewChat updateNewChat:
                {
                    if (!_firstGenerate)
                    {
                        if (_chatsIds.Contains(updateNewChat.Chat.Id))
                        {
                            ChatsList.DispatcherQueue.TryEnqueue(() =>
                            {
                                var chatEntry = new ChatEntry
                                {
                                    ChatPage = Chat,
                                    _chat = updateNewChat.Chat,
                                    ChatId = updateNewChat.Chat.Id
                                };
                                ChatsList.Items.Insert(0, chatEntry);
                                _chatsIds.Add(updateNewChat.Chat.Id);
                            });
                        }
                    }
                    break;
                }
                case TdApi.Update.UpdateChatDraftMessage updateChatDraftMessage:
                {
                    DispatcherQueue.TryEnqueue(() => UpdateChatPosition(updateChatDraftMessage.ChatId, ChatsList, ChatsList));
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private void UpdateArchivedChatsCount()
        {
            var chatsIds = _client.ExecuteAsync(new TdApi.GetChats
            {
                Limit = 100, ChatList = new TdApi.ChatList.ChatListArchive()
            }).Result.ChatIds;
            
            foreach (var chatId in chatsIds)
            {
                var chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId}).Result;
                if (chat.UnreadCount > 0) _totalUnreadArchivedChatsCount++;
            }
            
            ArchiveUnreadChats.Value = _totalUnreadArchivedChatsCount;
        }

        private void UpdateChatPosition(long chatId, ItemsControl chatsListToRemove, ItemsControl chatsListToInsert)
        {
            ChatsList.DispatcherQueue.EnqueueAsync(() =>
            {
                var chatToMove = ChatsList.Items
                    .OfType<ChatEntry>()
                    .FirstOrDefault(chat => chat.ChatId == chatId);
                if (chatToMove == null || chatToMove.ChatId != chatId) return;
                chatsListToRemove.Items.Remove(chatToMove);
                chatToMove.UpdateChatInfo();
                chatsListToInsert.Items.Insert(0, chatToMove);
            });
        }
        
        private async void OpenChat(long chatId)
        {
            try
            {
                var chat = await _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId});
            
                _currentChat = new Chat
                {
                    _ChatsView = this,
                    _chatId = chat.Id
                };
                await DispatcherQueue.GetForCurrentThread().EnqueueAsync(() =>
                {
                    _currentChat.UpdateChat(chat.Id);
                    Chat.Children.Add(_currentChat);
                });
            }
            catch
            {
                // ignored
            }
        }

        public void CloseChat()
        {
            if (_currentChat == null) return;
            _client.CloseChatAsync(_currentChat._chatId);
            Chat.Children.Remove(_currentChat);
        }
        
        private async Task GenerateChatEntries(TdApi.ChatList chatList)
        {
            PinnedChatsList.Items.Clear();
            ChatsList.Items.Clear();
            _pinnedChats.Clear();
            _chatsIds.Clear();
            _chats.Clear();
            
            _addedChats = await _client.GetChatsAsync(chatList, 10000).ConfigureAwait(false);
                     
            foreach (var chatId in _addedChats.ChatIds)
            {
                var chat = await _client.GetChatAsync(chatId);

                foreach (var chatPosition in chat.Positions)
                {
                    if (!chatPosition.IsPinned) continue;
                    if (_pinnedChats.Contains(chat.Id)) continue;
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
                    DispatcherQueue.TryEnqueue(() => Separator.Visibility = Visibility.Collapsed);
                else DispatcherQueue.TryEnqueue(() => Separator.Visibility = Visibility.Visible);

                await DispatcherQueue.EnqueueAsync(() =>
                {
                    ChatEntry chatEntry = new()
                    {
                        _ChatsView = this,
                        ChatPage = Chat,
                        _chat = chat,
                        ChatId = chat.Id,
                    };
                    chatEntry.UpdateChatInfo();

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

        private async void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSearch.Text == "")
            {
                if (_bInArchive)
                {
                    ArchiveStatus.Text = "Archive";
                    ArchiveUnreadChats.Visibility = Visibility.Visible;
                    _bInArchive = false;
                }
                GenerateChatEntries(new TdApi.ChatList.ChatListMain());
                return;
            }
            
            ChatsList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () => ChatsList.Items.Clear());
            
            var foundedChats = await _client.ExecuteAsync(new TdApi.SearchChats
            {
                Query = TextBoxSearch.Text,
                Limit = 100
            });

            var foundedMessages = await _client.ExecuteAsync(new TdApi.SearchMessages
            {
                ChatList = new TdApi.ChatList.ChatListMain(),
                Limit = 100,
                OnlyInChannels = true
            });
            
            
            foreach (var chatId in foundedChats.ChatIds)
            {
                var chat = _client.GetChatAsync(chatId).Result;
                
                var chatEntry = new ChatEntry
                {
                    _ChatsView = this,
                    ChatPage = Chat,
                    _chat = chat,
                    ChatId = chat.Id
                };
                    
                chatEntry.UpdateChatInfo();
                await DispatcherQueue.GetForCurrentThread().EnqueueAsync(() => ChatsList.Items.Add(chatEntry));
            }
        }

        private void ButtonArchive_OnClick(object sender, RoutedEventArgs e)
        {
            ChatsList.Items.Clear();
            if (!_bInArchive)
            {
                ArchiveStatus.Text = "Back";
                ArchiveUnreadChats.Visibility = Visibility.Collapsed;
                _bInArchive = true;
                GenerateChatEntries(new TdApi.ChatList.ChatListArchive());
            }
            else
            {
                ArchiveStatus.Text = "Archive";
                ArchiveUnreadChats.Visibility = Visibility.Visible;
                _bInArchive = false;
                GenerateChatEntries(new TdApi.ChatList.ChatListMain());
            }
        }

        private void ButtonSavedMessages_OnClick(object sender, RoutedEventArgs e)
        {
            OpenChat(_client.ExecuteAsync(new TdApi.GetMe()).Result.Id);
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
                    CloseChat();
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
    }
}