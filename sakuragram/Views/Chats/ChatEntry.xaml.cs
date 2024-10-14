using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
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
        
        private MediaService _mediaService = new();
        
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
                        DispatcherQueue.TryEnqueue(() => MediaService.GetChatPhoto(_chat, ChatEntryProfilePicture));
                    }
                    break;
                }
                case TdApi.Update.UpdateChatLastMessage updateChatLastMessage:
                {
                    if (updateChatLastMessage.LastMessage == null) break;
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (ChatId == updateChatLastMessage.LastMessage.ChatId)
                        {
                            string senderName = UserService.GetSenderName(_chat.LastMessage).Result;
                            TextBlockChatUsername.Text = senderName != string.Empty ? senderName + ": " : string.Empty;
                            TextBlockChatLastMessage.Text = MessageService.GetLastMessageContent(_chat.LastMessage).Result;
                        }
                    });
                    break;
                }
                case TdApi.Update.UpdateUnreadChatCount updateUnreadChatCount:
                {
                    UnreadMessagesCount.DispatcherQueue.TryEnqueue(() => UnreadMessagesCount.Value = updateUnreadChatCount.UnreadCount);
                    break;
                }
                case TdApi.Update.UpdateChatDraftMessage updateChatDraftMessage:
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (updateChatDraftMessage.ChatId == ChatId)
                        {
                            TextBlockChatLastMessage.Text = updateChatDraftMessage.DraftMessage.InputMessageText switch
                            {
                                TdApi.InputMessageContent.InputMessageText inputMessageText => "Draft:" + inputMessageText.Text.Text,
                                _ => "Draft message"
                            };
                            
                            TextBlockChatUsername.Text = string.Empty;
                            TextBlockChatUsername.Visibility = Visibility.Collapsed;
                        }
                    });
                    break;
                }
            }

            return Task.CompletedTask;
        }

        public async void UpdateChatInfo()
        {
            _chat = await _client.GetChatAsync(chatId: ChatId);
            string senderName = await UserService.GetSenderName(_chat.LastMessage).ConfigureAwait(false);
            
            MediaService.GetChatPhoto(_chat, ChatEntryProfilePicture);
            
            TextBlockChatName.Text = _chat.Title;
            TextBlockChatUsername.Text = senderName != string.Empty ? senderName + ": " : string.Empty;
            TextBlockChatLastMessage.Text = MessageService.GetLastMessageContent(_chat.LastMessage).Result;
            //GetLastMessage(_chat);
            
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
                    var supergroup = await _client.ExecuteAsync(new TdApi.GetSupergroup
                    {
                        SupergroupId = typeSupergroup.SupergroupId
                    }).ConfigureAwait(false);

                    if (supergroup is { IsForum: true })
                    {
                        var message = await _client.GetMessageAsync(chatId: ChatId, messageId: _chat.LastMessage.Id)
                            .ConfigureAwait(false);
                        
                        var topics = await _client.ExecuteAsync(new TdApi.GetForumTopics
                        {
                            ChatId = ChatId,
                            Limit = 100,
                            Query = "",
                            OffsetMessageId = message.Id,
                            OffsetMessageThreadId = message.MessageThreadId
                        }).ConfigureAwait(false);

                        if (message.IsTopicMessage)
                        {
                            var topic = await _client.ExecuteAsync(new TdApi.GetForumTopic
                            {
                                ChatId = ChatId,
                                MessageThreadId = message.MessageThreadId
                            }).ConfigureAwait(false);
                            
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                TextBlockForumName.Text = topic.Info.Name;
                                TextBlockForumName.Visibility = Visibility.Visible;
                            });
                        }
                        else
                        {
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                TextBlockForumName.Text = topics.Topics[0].Info.Name;
                                TextBlockForumName.Visibility = Visibility.Visible;
                            });
                        }
                    }
                    else
                    {
                        await DispatcherQueue.EnqueueAsync(() => TextBlockForumName.Visibility = Visibility.Collapsed);
                    }
                    break;
                }
            }

            try
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(_chat.LastMessage.Date).ToLocalTime();
                string sendTime = dateTime.ToShortTimeString();

                await DispatcherQueue.EnqueueAsync(() => TextBlockSendTime.Text = sendTime);
            }
            catch (Exception e)
            {
                await DispatcherQueue.EnqueueAsync(() => TextBlockSendTime.Text = e.Message);
            }
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
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
                _chatWidget.UpdateChat(_chat.Id);
                _ChatsView._currentChat = _chatWidget;
                ChatPage.Children.Add(_chatWidget);
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

            if (chat.DraftMessage != null)
            {
                TextBlockChatLastMessage.Text = chat.DraftMessage.InputMessageText switch
                {
                    TdApi.InputMessageContent.InputMessageText messageText => $"Draft: {messageText.Text.Text}",
                    _ => "Draft message"
                };
                TextBlockChatUsername.Text = string.Empty;
                TextBlockChatUsername.Visibility = Visibility.Collapsed;
            }
            else
            {
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
