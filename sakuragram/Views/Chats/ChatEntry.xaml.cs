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
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
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
                case TdApi.Update.UpdateNewMessage updateNewMessage:
                {
                    if (updateNewMessage.Message == null) break;
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (ChatId == updateNewMessage.Message.ChatId)
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
            string senderName = await UserService.GetSenderName(_chat.LastMessage);
            
            MediaService.GetChatPhoto(_chat, ChatEntryProfilePicture);
            
            TextBlockChatName.Text = _chat.Title;
            TextBlockChatUsername.Text = senderName != string.Empty ? senderName + ": " : string.Empty;
            TextBlockChatLastMessage.Text = MessageService.GetLastMessageContent(_chat.LastMessage).Result;
            TextBlockChatUsername.Visibility = TextBlockChatUsername.Text == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            TextBlockSendTime.Text = MathService.CalculateDateTime(_chat.LastMessage.Date).ToShortTimeString();
            
            if (_chat.UnreadCount > 0)
            {
                UnreadMessagesCount.Visibility = Visibility.Visible;
                UnreadMessagesCount.Value = _chat.UnreadCount;
            }
            else
            {
                UnreadMessagesCount.Visibility = Visibility.Collapsed;
            }

            if (_chat.NotificationSettings.MuteFor > 100000000)
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
        
        private void ChatEntry_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //ContextMenu.ShowAt(ButtonChatEntry);
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
