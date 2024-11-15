using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using sakuragram.Services;
using sakuragram.Services.Core;
using TdLib;

namespace sakuragram.Views.Chats;

public sealed partial class ChatEntry
{
    public Grid ChatPage;
    private static Chat _chatWidget;
    public ChatsView _ChatsView;

    private static readonly TdClient _client = App._client;
    private ChatService _chatService = App.ChatService;
        
    public TdApi.Chat _chat;
    public long ChatId;
    private int _profilePhotoFileId;
    private bool _inArchive;
    private string _lastMessageText = string.Empty;
    private int _unreadCount = 0;

    public ChatEntry()
    {
        InitializeComponent();
        Task.Run(async () => await UpdateAsync());
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        try
        {
            switch (update)
            {
                // case TdApi.Update.UpdateChatTitle updateChatTitle:
                // {
                //     if (updateChatTitle.ChatId == ChatId)
                //     {
                //         await DispatcherQueue.EnqueueAsync(() => TextBlockChatName.Text = updateChatTitle.Title);
                //     }
                //
                //     break;
                // }
                // case TdApi.Update.UpdateChatPhoto updateChatPhoto:
                // {
                //     if (updateChatPhoto.ChatId == ChatId)
                //     {
                //         await DispatcherQueue.EnqueueAsync(async () =>
                //             await MediaService.GetChatPhoto(_chat, ChatEntryProfilePicture));
                //     }
                //
                //     break;
                // }
                case TdApi.Update.UpdateChatLastMessage updateChatLastMessage:
                {
                    if (updateChatLastMessage.LastMessage.ChatId == _chat.Id)
                    {
                        await DispatcherQueue.EnqueueAsync(async () =>
                        {
                            string senderName = await UserService.GetSenderName(updateChatLastMessage.LastMessage);
                            TextBlockChatUsername.Text =
                                senderName != string.Empty ? senderName + ": " : string.Empty;
                            _lastMessageText =
                                TextService.Truncate(
                                    await MessageService.GetTextMessageContent(updateChatLastMessage.LastMessage),
                                    30);
                            TextBlockChatLastMessage.Text = _lastMessageText;
                            TextBlockChatUsername.Visibility = Visibility.Visible; 
                        });
                    }
                    break;
                }
                case TdApi.Update.UpdateNewMessage updateNewMessage:
                {
                    if (updateNewMessage.Message.ChatId == _chat.Id)
                    {
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            string time = MathService.CalculateDateTime(updateNewMessage.Message.Date).ToShortTimeString();
                            TextBlockSendTime.Text = time;
                            _unreadCount++;
                            UnreadMessagesCount.Visibility = _unreadCount >= 1 ? Visibility.Visible : Visibility.Collapsed;
                            UnreadMessagesCount.Value = _unreadCount;
                        });
                    }
                    break;
                }
                case TdApi.Update.UpdateChatDraftMessage updateChatDraftMessage:
                {
                    if (updateChatDraftMessage.ChatId == _chat.Id)
                    {
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            TextBlockChatLastMessage.Text =
                                updateChatDraftMessage.DraftMessage.InputMessageText switch
                                {
                                    TdApi.InputMessageContent.InputMessageText inputMessageText => "Draft:" +
                                        inputMessageText.Text.Text,
                                    _ => "Draft message"
                                };
            
                            TextBlockChatUsername.Text = string.Empty;
                            TextBlockChatUsername.Visibility = Visibility.Collapsed;
                        });
                    }
                    break;
                }
                case TdApi.Update.UpdateChatAction updateChatAction:
                {
                    if (updateChatAction.ChatId == _chat.Id)
                    {
                        var sender = await UserService.GetSender(updateChatAction.SenderId);
                        var senderName = sender.User != null
                            ?  string.IsNullOrEmpty(sender.User.LastName) 
                                ? sender.User.FirstName 
                                : sender.User.FirstName + " " + sender.User.LastName
                            : sender.Chat?.Title;
                        
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            TextBlockChatUsername.Visibility = updateChatAction.Action switch
                            {
                                TdApi.ChatAction.ChatActionCancel => Visibility.Visible,
                                _ => Visibility.Collapsed
                            };
                            TextBlockChatLastMessage.Text = updateChatAction.Action switch
                            {
                                TdApi.ChatAction.ChatActionTyping => $"{senderName} typing...",
                                TdApi.ChatAction.ChatActionRecordingVideo => $"{senderName} recording video...",
                                TdApi.ChatAction.ChatActionUploadingVideo => $"{senderName} uploading video...",
                                TdApi.ChatAction.ChatActionRecordingVoiceNote =>
                                    $"{senderName} recording voice note...",
                                TdApi.ChatAction.ChatActionUploadingVoiceNote =>
                                    $"{senderName} uploading voice note...",
                                TdApi.ChatAction.ChatActionUploadingPhoto => $"{senderName} uploading photo...",
                                TdApi.ChatAction.ChatActionUploadingDocument =>
                                    $"{senderName} uploading document...",
                                TdApi.ChatAction.ChatActionUploadingVideoNote =>
                                    $"{senderName} uploading video note...",
                                TdApi.ChatAction.ChatActionChoosingContact => $"{senderName} choosing contact...",
                                TdApi.ChatAction.ChatActionChoosingLocation => $"{senderName} choosing location...",
                                TdApi.ChatAction.ChatActionChoosingSticker => $"{senderName} choosing sticker...",
                                TdApi.ChatAction.ChatActionWatchingAnimations =>
                                    $"{senderName} watching animations...",
                                TdApi.ChatAction.ChatActionRecordingVideoNote =>
                                    $"{senderName} recording video note...",
                                TdApi.ChatAction.ChatActionStartPlayingGame => $"{senderName} playing game...",
                                _ => "Unknown"
                            };
                            
                        });
                    }
                    break;
                }
                case TdApi.Update.UpdateChatUnreadMentionCount updateChatUnreadMentionCount:
                {
                    if (updateChatUnreadMentionCount.ChatId == _chat.Id)
                    {
                        await DispatcherQueue.EnqueueAsync(() => 
                            BorderMention.Visibility = updateChatUnreadMentionCount.UnreadMentionCount > 0
                                ? Visibility.Visible
                                : Visibility.Collapsed);
                    }
                    break;
                }
                case TdApi.Update.UpdateMessageMentionRead updateMessageMentionRead:
                {
                    if (updateMessageMentionRead.ChatId == _chat.Id)
                    {
                        await DispatcherQueue.EnqueueAsync(() => 
                            BorderMention.Visibility = updateMessageMentionRead.UnreadMentionCount > 0
                                ? Visibility.Visible
                                : Visibility.Collapsed);
                    }
                    break;
                }
                case TdApi.Update.UpdateChatUnreadReactionCount updateChatUnreadReactionCount:
                {
                    if (updateChatUnreadReactionCount.ChatId == _chat.Id)
                    {
                        await DispatcherQueue.EnqueueAsync(() => 
                            BorderReaction.Visibility = updateChatUnreadReactionCount.UnreadReactionCount > 0
                                ? Visibility.Visible
                                : Visibility.Collapsed);
                    }
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private async Task UpdateAsync()
    {
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        
        _chat = await _client.GetChatAsync(ChatId);
        var currentUser = await _client.GetMeAsync();
        string senderName = await UserService.GetSenderName(_chat.LastMessage);

        await DispatcherQueue.EnqueueAsync(async () => {
            await MediaService.GetChatPhoto(_chat, ChatEntryProfilePicture);
            TextBlockChatName.Text = TextService.Truncate(_chat.Title, 38);
            TextBlockChatUsername.Text = senderName != string.Empty ? senderName + ": " : string.Empty;
            var text = await MessageService.GetTextMessageContent(_chat.LastMessage);
            _lastMessageText = TextService.Truncate(text.Normalize(NormalizationForm.FormKC), 38);
            TextBlockChatLastMessage.Text = _lastMessageText;
            TextBlockChatUsername.Visibility = TextBlockChatUsername.Text == string.Empty
                ? Visibility.Collapsed
                : Visibility.Visible;
        });
        
        if (_chat.NotificationSettings.MuteFor > 0)
        {
            await DispatcherQueue.EnqueueAsync(() => 
                UnreadMessagesCount.Background = new SolidColorBrush(Colors.Gray));
        }

        #region Unread

        await DispatcherQueue.EnqueueAsync(() => BorderMention.Visibility =  _chat.UnreadMentionCount > 0 ? Visibility.Visible : Visibility.Collapsed);
        await DispatcherQueue.EnqueueAsync(() => BorderReaction.Visibility =  _chat.UnreadReactionCount > 0 ? Visibility.Visible : Visibility.Collapsed);
        
        if (_chat.UnreadCount > 0)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                UnreadMessagesCount.Visibility = Visibility.Visible;
                _unreadCount = _chat.UnreadCount;
                UnreadMessagesCount.Value = _unreadCount;
            });
        }
        else
        {
            await DispatcherQueue.EnqueueAsync(() => UnreadMessagesCount.Visibility = Visibility.Collapsed);
        }

        #endregion

        #region SendTime

        try
        {
            var today = DateTime.UtcNow.Date;
            var lastMessageDate = MathService.CalculateDateTime(_chat.LastMessage.Date);

            var timeDiff = (today - lastMessageDate.Date).TotalHours;

            if (timeDiff < 24)
            {
                await DispatcherQueue.EnqueueAsync(() => TextBlockSendTime.Text = lastMessageDate.ToString("HH:mm"));
            }
            else if (timeDiff < 168) 
            {
                await DispatcherQueue.EnqueueAsync(() => TextBlockSendTime.Text = lastMessageDate.ToString("ddd"));
            }
            else
            {
                await DispatcherQueue.EnqueueAsync(() => TextBlockSendTime.Text = lastMessageDate.ToString("M.d.yy"));
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        #endregion

        #region ReadStatus

        var sender = await UserService.GetSender(_chat.LastMessage.SenderId);
        if (sender.User != null)
        {
            if (sender.User.Id == currentUser.Id)
            {
                if (_chat.LastMessage.Id >= _chat.LastReadInboxMessageId)
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        IconReadStatus.Glyph = "\uE9D5";
                        IconReadStatus.Visibility = Visibility.Visible;
                    });
                }
                else
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        IconReadStatus.Glyph = "\uE73E";
                        IconReadStatus.Visibility = Visibility.Visible;
                    });
                }
            }
        }

        #endregion
        
        switch (_chat.Type)
        {
            case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
            {
                var supergroup = await _client.ExecuteAsync(new TdApi.GetSupergroup
                {
                    SupergroupId = typeSupergroup.SupergroupId
                });
            
                if (supergroup is { IsForum: true })
                {
                    var message = await _client.GetMessageAsync(ChatId, _chat.LastMessage.Id);
            
                    var topics = await _client.ExecuteAsync(new TdApi.GetForumTopics
                    {
                        ChatId = ChatId,
                        Limit = 100,
                        Query = "",
                        OffsetMessageId = message.Id,
                        OffsetMessageThreadId = message.MessageThreadId
                    });
            
                    if (message.IsTopicMessage)
                    {
                        var topic = await _client.ExecuteAsync(new TdApi.GetForumTopic
                        {
                            ChatId = ChatId,
                            MessageThreadId = message.MessageThreadId
                        });
            
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
                    await DispatcherQueue.EnqueueAsync(() =>
                        TextBlockForumName.Visibility = Visibility.Collapsed);
                }

                await DispatcherQueue.EnqueueAsync(() =>
                {
                    switch (supergroup)
                    {
                        case { IsForum: true }:
                            IconChatType.Glyph = "\uE902";
                            IconChatType.Visibility = Visibility.Visible;
                            break;
                        case { IsChannel: true }:
                            IconChatType.Glyph = "\uEC24";
                            IconChatType.Visibility = Visibility.Visible;
                            break;
                        case { IsForum: false, IsChannel: false }:
                            IconChatType.Glyph = "\uE716";
                            IconChatType.Visibility = Visibility.Visible;
                            break;
                    }
                });
                break;
            }
            case TdApi.ChatType.ChatTypeBasicGroup:
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    IconChatType.Glyph = "\uE716";
                    IconChatType.Visibility = Visibility.Visible;
                });
                break;
            }
            case TdApi.ChatType.ChatTypePrivate typePrivate:
            {
                var user = await _client.GetUserAsync(typePrivate.UserId);
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    switch (user.Type)
                    {
                        case TdApi.UserType.UserTypeBot:
                            IconChatType.Glyph = "\uE99A";
                            IconChatType.Visibility = Visibility.Visible;
                            break;
                        case TdApi.UserType.UserTypeRegular:
                            IconChatType.Visibility = Visibility.Collapsed;
                            break;
                        case TdApi.UserType.UserTypeDeleted:
                            IconChatType.Visibility = Visibility.Collapsed;
                            TextBlockChatName.Text = "Deleted Account";
                            break;
                        case TdApi.UserType.UserTypeUnknown:
                            IconChatType.Visibility = Visibility.Collapsed;
                            TextBlockChatName.Text = "Unknown Account";
                            break;
                        default:
                            TextBlockChatName.Text = "watafuck";
                            break;
                    }
                });
                break;
            }
            case TdApi.ChatType.ChatTypeSecret:
            {
                IconChatType.Glyph = "\uE72E";
                IconChatType.Visibility = Visibility.Visible; 
                break;
            }
        }
    }
        
    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        _ChatsView.CloseChat();
        switch (_chat.Type)
        {
            case TdApi.ChatType.ChatTypePrivate 
                or TdApi.ChatType.ChatTypeSecret 
                or TdApi.ChatType.ChatTypeBasicGroup:
                DispatcherQueue.EnqueueAsync(async () =>
                {
                    _chatWidget = await _chatService.OpenChat(_chat.Id);
                    _chatWidget._ChatsView = _ChatsView;
                    _ChatsView._currentChat = _chatWidget;
                    ChatPage.Children.Add(_chatWidget);
                });
                break;
            case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                var supergroup = _client.GetSupergroupAsync(typeSupergroup.SupergroupId).Result;
                if (supergroup.IsForum)
                {
                    _ChatsView.OpenForum(supergroup, _chat);
                }
                else
                {
                    DispatcherQueue.EnqueueAsync(async () =>
                    {
                        _chatWidget = await _chatService.OpenChat(_chat.Id);
                        _chatWidget._ChatsView = _ChatsView;
                        _ChatsView._currentChat = _chatWidget;
                        ChatPage.Children.Add(_chatWidget);
                    });
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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