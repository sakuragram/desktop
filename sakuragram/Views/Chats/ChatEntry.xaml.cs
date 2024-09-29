using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace sakuragram.Views.Chats
{
    public sealed partial class ChatEntry : Button
    {
        public Grid ChatPage;
        private static Chat _chatWidget;
        public ChatsView _ChatsView;

        private static readonly TdClient _client = App._client;
        
        public TdApi.Chat _chat;
        public long ChatId;
        private int _profilePhotoFileId;
        private bool _inArchive;
        
        public ChatEntry()
        {
            InitializeComponent();
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateChatTitle updateChatTitle:
                {
                    if (updateChatTitle.ChatId == ChatId)
                    {
                        TextBlockChatName.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal,
                            () => TextBlockChatName.Text = updateChatTitle.Title);
                    }
                    break;
                }
                case TdApi.Update.UpdateChatPhoto updateChatPhoto:
                {
                    if (updateChatPhoto.ChatId == ChatId)
                    {
                        ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(() => GetChatPhoto(_chat));
                    }
                    break;
                }
                case TdApi.Update.UpdateFile updateFile:
                {
                    if (updateFile.File.Id != _profilePhotoFileId) return Task.CompletedTask;
                    ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(() =>
                    {
                        if (updateFile.File.Local.Path != "")
                        {
                            ChatEntryProfilePicture.ProfilePicture = new BitmapImage(new Uri(updateFile.File.Local.Path));
                        }
                        else
                        {
                            ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, 
                                () => ChatEntryProfilePicture.DisplayName = _chat.Title);
                        }
                    });
                    break;
                }
                case TdApi.Update.UpdateChatLastMessage updateChatLastMessage:
                {
                    TextBlockChatLastMessage.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
                    {
                        if (updateChatLastMessage.LastMessage == null) return;
                        if (ChatId == updateChatLastMessage.LastMessage.ChatId)
                        {
                            GetLastMessage(_client.GetChatAsync(updateChatLastMessage.ChatId).Result);
                        }
                    });
                    break;
                }
                case TdApi.Update.UpdateUnreadChatCount updateUnreadChatCount:
                {
                    UnreadMessagesCount.DispatcherQueue.TryEnqueue(() => UnreadMessagesCount.Value = updateUnreadChatCount.UnreadCount);
                    break;
                }
            }

            return Task.CompletedTask;
        }

        public async void UpdateChatInfo()
        {
            _chat = await _client.GetChatAsync(chatId: ChatId);
            TextBlockChatName.Text = _chat.Title;
            
            GetChatPhoto(_chat);
            GetLastMessage(_chat); 
            
            if (_chat.UnreadCount > 0)
            {
                if (UnreadMessagesCount.Visibility == Visibility.Collapsed) UnreadMessagesCount.Visibility = Visibility.Visible;
                UnreadMessagesCount.Value = _chat.UnreadCount;
            }
            else
            {
                UnreadMessagesCount.Visibility = Visibility.Collapsed;
            }

            if (_chat.NotificationSettings.MuteFor <= 0)
            {
                UnreadMessagesCount.Background = new SolidColorBrush(Colors.Gray);
            }
            
            switch (_chat.Type)
            {
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                {
                    // var supergroup = _client.GetSupergroupAsync(
                    //         supergroupId: typeSupergroup.SupergroupId).Result;
                    // if (supergroup.IsForum)
                    // {
                    //     var topic = _client.ExecuteAsync(new TdApi.GetForumTopic
                    //     {
                    //         ChatId = ChatId,
                    //         MessageThreadId = _chat.LastMessage.MessageThreadId
                    //     }).Result;
                    //     TextBlockForumName.Text = topic.Info.Name;
                    //     TextBlockForumName.Visibility = Visibility.Visible;
                    // }
                    // else
                    // {
                    //     TextBlockForumName.Visibility = Visibility.Collapsed;
                    // }
                    break;
                }
            }

            try
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(_chat.LastMessage.Date).ToLocalTime();
                string sendTime = dateTime.ToShortTimeString();

                TextBlockSendTime.Text = sendTime;
            }
            catch (Exception e)
            {
                TextBlockSendTime.Text = e.Message;
            }
            
            // foreach (var chatList in _chat.ChatLists)
            // {
            //     _inArchive = chatList switch
            //     {
            //         TdApi.ChatList.ChatListMain => false,
            //         TdApi.ChatList.ChatListArchive => true,
            //         _ => _inArchive
            //     };
            // }

            // ContextMenuArchive.Text = _inArchive ? "Unarchive" : "Archive";
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }
        
        private void GetChatPhoto(TdApi.Chat chat)
        {
            if (chat.Photo == null)
            {
                ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, 
                    () => ChatEntryProfilePicture.DisplayName = chat.Title);
                return;
            }
            if (chat.Photo.Small.Local.Path != "")
            {
                try
                {
                    ChatEntryProfilePicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low,
                        () => ChatEntryProfilePicture.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Small.Local.Path)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                _profilePhotoFileId = chat.Photo.Small.Id;
                
                var file = _client.ExecuteAsync(new TdApi.DownloadFile
                {
                    FileId = _profilePhotoFileId,
                    Priority = 1
                }).Result;
            }
        }

        private static string GetMessageText(long chatId, long messageId)
        {
            if (_client.ExecuteAsync(new TdApi.GetMessage
                {
                    ChatId = chatId,
                    MessageId = messageId
                }).Result.Content is not TdApi.MessageContent.MessageText message) return null;
            var messageText = message.Text.Text;
            return messageText;
        }
        
        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            _ChatsView.CloseChat();
            DispatcherQueue.GetForCurrentThread().EnqueueAsync(() =>
            {
                _chatWidget = new Chat();
                _chatWidget._ChatsView = _ChatsView;
                _chatWidget._chatId = ChatId;
                _chatWidget._chat = _client.GetChatAsync(ChatId).Result;
                _ChatsView._currentChat = _chatWidget;
                _chatWidget.UpdateChat(_chat.Id);
                ChatPage.Children.Add(_chatWidget);
                
                if (ContextMenu.IsOpen)
                {
                    ContextMenu.Hide();
                }
            });
        }
        
        private async void GetLastMessage(TdApi.Chat chat)
        {
            var currentUser = await _client.GetMeAsync();
            
            switch (chat.Type)
            {
                case TdApi.ChatType.ChatTypePrivate:
                    var privateId = chat.LastMessage.SenderId switch {
                        TdApi.MessageSender.MessageSenderUser u => u.UserId,
                        TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                        _ => 0
                    };
                    
                    if (privateId == currentUser.Id)
                    {
                        TextBlockChatUsername.Visibility = Visibility.Visible;
                        TextBlockChatUsername.Text = "You: ";
                    }
                    else
                    {
                        TextBlockChatUsername.Visibility = Visibility.Collapsed;
                    }
                    
                    break;
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                {
                    var supergroup = await _client.GetSupergroupAsync(supergroupId: typeSupergroup.SupergroupId);
                    
                    if (supergroup.IsChannel)
                    {
                        TextBlockChatUsername.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        TextBlockChatUsername.Visibility = Visibility.Visible;
                        try
                        {
                            var id = chat.LastMessage.SenderId switch {
                                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                                _ => 0
                            };
            
                            if (id == currentUser.Id)
                            {
                                TextBlockChatUsername.Visibility = Visibility.Visible;
                                TextBlockChatUsername.Text = "You: ";
                            }
                            
                            if (id > 0)
                            {
                                var user = await _client.GetUserAsync(id);
                                TextBlockChatUsername.Text = user.FirstName + ": ";
                            }
                            else
                            {
                                TextBlockChatUsername.Text = chat.Title + ": ";
                            }
                        }
                        catch {}
                    }
                    
                    break;
                }
            }
            
            var sender = chat.LastMessage.SenderId switch
            {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };
            
            TextBlockChatLastMessage.Text = chat.LastMessage.Content switch
            {
                TdApi.MessageContent.MessageText messageText => $"{messageText.Text.Text}",
                TdApi.MessageContent.MessageAnimation messageAnimation => 
                    $"GIF ({messageAnimation.Animation.Duration} sec), {messageAnimation.Caption.Text}",
                TdApi.MessageContent.MessageAudio messageAudio =>
                    $"Audio message ({messageAudio.Audio.Duration} sec) {messageAudio.Caption.Text}",
                TdApi.MessageContent.MessageVoiceNote messageVoiceNote =>
                    $"Voice message ({messageVoiceNote.VoiceNote.Duration} sec) {messageVoiceNote.Caption.Text}",
                TdApi.MessageContent.MessageVideo messageVideo =>
                    $"Video ({messageVideo.Video.Duration} sec)",
                TdApi.MessageContent.MessageVideoNote messageVideoNote =>
                    $"Video message ({messageVideoNote.VideoNote.Duration} sec)",
                TdApi.MessageContent.MessagePhoto messagePhoto =>
                    $"Photo, {messagePhoto.Caption.Text}",
                TdApi.MessageContent.MessageSticker messageSticker =>
                    $"{messageSticker.Sticker.Emoji} Sticker message",
                TdApi.MessageContent.MessagePoll messagePoll => $"📊 {messagePoll.Poll.Question.Text}",
                TdApi.MessageContent.MessagePinMessage messagePinMessage =>
                    $"pinned {GetMessageText(chat.Id, messagePinMessage.MessageId)}",
                
                // Chat messages
                TdApi.MessageContent.MessageChatAddMembers messageChatAddMembers => $"{messageChatAddMembers.MemberUserIds}",
                TdApi.MessageContent.MessageChatChangeTitle messageChatChangeTitle => $"changed chat title to {messageChatChangeTitle.Title}",
                TdApi.MessageContent.MessageChatChangePhoto => "updated group photo",
                
                TdApi.MessageContent.MessageChatDeleteMember messageChatDeleteMember => 
                    $"removed user {_client.GetUserAsync(userId: messageChatDeleteMember.UserId).Result.FirstName}",
                TdApi.MessageContent.MessageChatDeletePhoto => $"deleted group photo",
                
                TdApi.MessageContent.MessageChatUpgradeFrom messageChatUpgradeFrom => 
                    $"{messageChatUpgradeFrom.Title} upgraded to supergroup",
                TdApi.MessageContent.MessageChatUpgradeTo messageChatUpgradeTo => $"",
                
                TdApi.MessageContent.MessageChatJoinByLink => $"joined by link",
                TdApi.MessageContent.MessageChatJoinByRequest => $"joined by request",
                
                _ => "Unsupported message type"
            };
        }
        
        private void ChatEntry_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ContextMenu.ShowAt(ButtonChatEntry);
        }

        private void ContextMenuMarkAs_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ContextMenuNotifications_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ContextMenuPin_OnClick(object sender, RoutedEventArgs e)
        {
            //_client.ExecuteAsync()
        }

        private void ContextMenuArchive_OnClick(object sender, RoutedEventArgs e)
        {
            if (_inArchive)
            {
                _client.ExecuteAsync(new TdApi.AddChatToList
                {
                    ChatId = ChatId,
                    ChatList = new TdApi.ChatList.ChatListArchive()
                });
            }
            else
            {
                _client.ExecuteAsync(new TdApi.AddChatToList
                {
                    ChatId = ChatId,
                    ChatList = new TdApi.ChatList.ChatListMain()
                });
            }
        }

        private void ContextMenuLeave_OnClick(object sender, RoutedEventArgs e)
        {
            LeaveChatConfirmation.ShowAsync();
        }

        private void LeaveChatConfirmation_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _client.ExecuteAsync(new TdApi.LeaveChat { ChatId = ChatId });
        }
    }
}