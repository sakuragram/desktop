using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Controls.Messages;
using sakuragram.Services;
using sakuragram.Services.Chat;
using sakuragram.Services.Core;
using sakuragram.Views.Calls;
using sakuragram.Views.Chats.Messages;
using TdLib;

namespace sakuragram.Views.Chats;

public sealed partial class Chat : Page
{
    private static TdClient _client = App._client;
    private static ChatService _chatService = App.ChatService;
    public TdApi.Chat _chat;
    private TdApi.ForumTopic _forumTopic;
    private static TdApi.Background _background;
    public ChatsView _ChatsView;
    private List<TdApi.FormattedText> _pollOptionsList = [];

    private long _chatId = _chatService._openedChatId;
    public long _linkedChatId;
    private string _backgroundUrl;
    private int _backgroundId;
    private int _memberCount;
    private int _onlineMemberCount;
    private int _offset;
    private long _lastMessageId;
    private int _pollOptionsCount = 2;
    private bool _isProfileOpened = false;
    private bool _hasInternetConnection = true;
    private bool _isForum = false;
    public bool _mediaMenuOpened = false;

    #region LoadProperties

    private bool _isNeedLoadFastActions = true;

    #endregion
        
    private ReplyService _replyService = new ReplyService();
    private MessageService _messageService;
    
    private int _chatActionRows = 0;
    private int _chatActionColumns = 0;
    
    private List<TdApi.StickerSet> _stickerSets = [];
    private List<TdApi.StickerSet> _alreadyAddedStickerSets = [];
    private int _currentStickerSet = -1;
    private int _currentMediaPage;

    public Chat(long id, bool isForum, TdApi.ForumTopic forumTopic)
    {
        InitializeComponent();
        
        _chatId = id;
        _isForum = isForum;
        _forumTopic = forumTopic;
        _replyService = new ReplyService();
        _messageService = new MessageService();

        Task.Run(async () => await UpdateChat());
        
        #if DEBUG
        {
            CreateVideoCall.IsEnabled = true;
        }
        #else
        {
            CreateVideoCall.IsEnabled = false;
        }
        #endif
        
        ColumnMediaPanel.Width = new GridLength(0);
        
        BarItemEmojis.PointerPressed += (_, _) => SelectMediaPage(0);
        BarItemStickers.PointerPressed += (_, _) => SelectMediaPage(1);
        BarItemAnimations.PointerPressed += (_, _) => SelectMediaPage(2);
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateNewMessage updateNewMessage:
            {
                if (updateNewMessage.Message.ChatId == _chatId)
                {
                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        // For debug
                        if (FeaturesManager._debugMode)
                        {
                            var message = new ChatDebugMessage();
                            MessagesList.Children.Add(message);
                            message.UpdateMessage(updateNewMessage.Message);
                        }
                        else
                        {
                            await GenerateMessageByType([updateNewMessage.Message]);
                        }

                        await DispatcherQueue.EnqueueAsync(() => 
                            MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight));
                    });
                }
                break;
            }
            case TdApi.Update.UpdateChatAction updateChatAction:
            {
                if (updateChatAction.ChatId == _chatId)
                {
                    var sender = await UserService.GetSender(updateChatAction.SenderId);
                    var senderName = sender.User != null 
                        ? string.IsNullOrEmpty(sender.User.LastName) 
                            ? sender.User.FirstName 
                            : $"{sender.User.FirstName} {sender.User.LastName}"
                        : sender.Chat?.Title;
                    
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        OnlineChatMembers.Text = updateChatAction.Action switch
                        {
                            TdApi.ChatAction.ChatActionCancel => _onlineMemberCount > 0
                                ? $", online {_onlineMemberCount}"
                                : string.Empty,
                            TdApi.ChatAction.ChatActionTyping => $"{senderName} typing...",
                            TdApi.ChatAction.ChatActionRecordingVideo => $"{senderName} recording video...",
                            TdApi.ChatAction.ChatActionUploadingVideo => $"{senderName} uploading video...",
                            TdApi.ChatAction.ChatActionRecordingVoiceNote => $"{senderName} recording voice note...",
                            TdApi.ChatAction.ChatActionUploadingVoiceNote => $"{senderName} uploading voice note...",
                            TdApi.ChatAction.ChatActionUploadingPhoto => $"{senderName} uploading photo...",
                            TdApi.ChatAction.ChatActionUploadingDocument => $"{senderName} uploading document...",
                            TdApi.ChatAction.ChatActionUploadingVideoNote => $"{senderName} uploading video note...",
                            TdApi.ChatAction.ChatActionChoosingContact => $"{senderName} choosing contact...",
                            TdApi.ChatAction.ChatActionChoosingLocation => $"{senderName} choosing location...",
                            TdApi.ChatAction.ChatActionChoosingSticker => $"{senderName} choosing sticker...",
                            TdApi.ChatAction.ChatActionWatchingAnimations => $"{senderName} watching animations...",
                            TdApi.ChatAction.ChatActionRecordingVideoNote => $"{senderName} recording video note...",
                            TdApi.ChatAction.ChatActionStartPlayingGame => $"{senderName} playing game...",
                            _ => "Unknown"
                        };

                        ChatMembers.Visibility = updateChatAction.Action switch
                        {
                            TdApi.ChatAction.ChatActionCancel => Visibility.Visible,
                            _ => Visibility.Collapsed
                        };
                    });
                }
                break;
            }
            case TdApi.Update.UpdateChatTitle updateChatTitle:
            {
                if (updateChatTitle.ChatId == _chatId)
                    await DispatcherQueue.EnqueueAsync(() => ChatTitle.Text = updateChatTitle.Title);
                break;
            }
            case TdApi.Update.UpdateUserStatus updateUserStatus:
            {
                if (updateUserStatus.UserId != _chat.Id) return;
                if (_chat.Type is TdApi.ChatType.ChatTypePrivate)
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        var status = updateUserStatus.Status switch
                        {
                            TdApi.UserStatus.UserStatusOnline => "Online",
                            TdApi.UserStatus.UserStatusOffline => "Offline",
                            TdApi.UserStatus.UserStatusRecently => "Last seen recently",
                            TdApi.UserStatus.UserStatusLastWeek => "Last week",
                            TdApi.UserStatus.UserStatusLastMonth => "Last month",
                            TdApi.UserStatus.UserStatusEmpty => "Last seen a long time ago",
                            _ => "Unknown"
                        };
        
                        ChatMembers.Text = status;
        
                        if (_isProfileOpened)
                        {
                            TextBlockMembersOrStatus.Text = status;
                        }
                    });
                }
                break;
            }
            case TdApi.Update.UpdateChatOnlineMemberCount updateChatOnlineMemberCount:
            {
                if (_chat.Type is TdApi.ChatType.ChatTypeSupergroup or TdApi.ChatType.ChatTypeBasicGroup &&
                    _chat.Permissions.CanSendBasicMessages)
                {
                    _onlineMemberCount = updateChatOnlineMemberCount.OnlineMemberCount;
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        if (_onlineMemberCount > 0)
                        {
                            OnlineChatMembers.Text = $", {_onlineMemberCount} online";
                            OnlineChatMembers.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            OnlineChatMembers.Text = string.Empty;
                            OnlineChatMembers.Visibility = Visibility.Collapsed;
                        }
                    });   
                }
                break;
            }
            case TdApi.Update.UpdateBasicGroup updateBasicGroup:
            {
                if (updateBasicGroup.BasicGroup.Id == _chatId)
                {
                    _memberCount = updateBasicGroup.BasicGroup.MemberCount;
                    await DispatcherQueue.EnqueueAsync(() => ChatMembers.Text = $"{_memberCount} members");
                }
                break;
            }
            case TdApi.Update.UpdateSupergroup updateSupergroup:
            {
                if (updateSupergroup.Supergroup.Id == _chatId)
                {
                    _memberCount = updateSupergroup.Supergroup.MemberCount;
                    await DispatcherQueue.EnqueueAsync(() => ChatMembers.Text = $"{_memberCount} members");
                }
                break;
            }
            case TdApi.Update.UpdateDeleteMessages updateDeleteMessages:
            {
                // if (updateDeleteMessages.ChatId == _chatId)
                // {
                //     var messages = MessagesList.Children;
                //     var messagesToRemove = messages.OfType<ChatMessage>();
                //     
                //     foreach (var messageId in updateDeleteMessages.MessageIds)
                //     {
                //         MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
                //         {
                //             foreach (var messageToRemove in messagesToRemove)
                //             {
                //                 if (messageToRemove._messageId == messageId)
                //                 {
                //                     MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low,
                //                         () => MessagesList.Children.Remove(messageToRemove));
                //                 }
                //             }
                //         });
                //     }
                // }
                break;
            }
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id == _backgroundId)
                {
                    if (_background.Document.Document_.Local.Path != string.Empty)
                    {
                        ThemeBackground.DispatcherQueue.TryEnqueue(() => 
                            ThemeBackground.ImageSource = new BitmapImage(
                                new Uri(_background.Document.Document_.Local.Path)
                            ));
                    }
                    else if (updateFile.File.Local.Path != string.Empty)
                    {
                        ThemeBackground.DispatcherQueue.TryEnqueue(() => 
                            ThemeBackground.ImageSource = new BitmapImage(
                                new Uri(updateFile.File.Local.Path)
                            ));
                    }
                }
                break;
            }
            case TdApi.Update.UpdateConnectionState updateConnectionState:
            {
                _hasInternetConnection = updateConnectionState.State switch
                {
                    TdApi.ConnectionState.ConnectionStateReady => true,
                    TdApi.ConnectionState.ConnectionStateConnecting => false,
                    TdApi.ConnectionState.ConnectionStateUpdating => false,
                    TdApi.ConnectionState.ConnectionStateConnectingToProxy => false,
                    TdApi.ConnectionState.ConnectionStateWaitingForNetwork => false,
                    _ => false
                };
                break;
            }
        }
    }

    public Visibility UpdateMediaVisibility()
    {
        return Media.Visibility = _mediaMenuOpened ? Visibility.Visible : Visibility.Collapsed;
    }

    public GridLength UpdateMediaLength()
    {
        return ColumnMediaPanel.Width = _mediaMenuOpened ? new GridLength(350) : new GridLength(0);
    }
    
    public async Task UpdateChat()
    {
        try
        {
            await _client.ExecuteAsync(new TdApi.OpenChat { ChatId = _chatService._openedChatId });
            _chat = await _client.GetChatAsync(_chatService._openedChatId);
            if (_chat == null) return;
            await DispatcherQueue.EnqueueAsync(() => ChatTitle.Text = _isForum 
                ? _forumTopic.Info.Name
                : _chat.Title);
            
            if (_isForum) ChatPhoto.Visibility = Visibility.Collapsed;
            else await ChatPhoto.InitializeProfilePhoto(null, _chat, 30, 30);

            try
            {
                var pinnedMessage = await _client.GetChatPinnedMessageAsync(_chat.Id);
                if (pinnedMessage != null)
                {
                    var pinText = await MessageService.GetTextMessageContent(pinnedMessage);
                    pinText = Regex.Replace(pinText, @"\s+", " ");
                    pinText = pinText.Trim();
                    await DispatcherQueue.EnqueueAsync(() => TextBlockPinnedMessage.Text = pinText);
                }
                else
                {
                    await DispatcherQueue.EnqueueAsync(() => BorderPinnedMessage.Visibility = Visibility.Collapsed);
                }
            }
            catch (TdException e)
            {
                Logger.Log(Logger.LogType.Error, e);
                await DispatcherQueue.EnqueueAsync(() => BorderPinnedMessage.Visibility = Visibility.Collapsed);
            }

            {
                {
                    {
            
            if (_isForum && _forumTopic.DraftMessage != null)
            {
                PrepareDraftMessage(_forumTopic.DraftMessage);
            }
            else if (_chat.DraftMessage != null)
            {
                PrepareDraftMessage(_chat.DraftMessage);
            }

            if (_chat.Background != null)
            {
                _background = _chat.Background.Background;
                _backgroundId = _background.Document.Document_.Id;

                if (_background.Document.Document_.Local.Path != string.Empty)
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                        ThemeBackground.ImageSource =
                            new BitmapImage(new Uri(_background.Document.Document_.Local.Path)));
                }
                else
                {
                    await _client.ExecuteAsync(new TdApi.DownloadFile
                    {
                        FileId = _backgroundId,
                        Priority = 1
                    });
                }
            }
            
            if (_chat.UnreadCount > 0 || _chat.UnreadMentionCount > 0 || _chat.UnreadReactionCount > 0)
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    ColumnUnreadActions.Width = new GridLength(50);
                    PanelUnreadActions.Visibility = Visibility.Visible;
                });
            }
            else
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    ColumnUnreadActions.Width = new GridLength(0);
                    PanelUnreadActions.Visibility = Visibility.Collapsed;
                });
            }
            
            if (_chat.UnreadCount > 0)
            {
                await DispatcherQueue.EnqueueAsync(() => ButtonUnreadMessages.Visibility = Visibility.Visible);
            }
            if (_chat.UnreadMentionCount > 0)
            {
                await DispatcherQueue.EnqueueAsync(() => ButtonUnreadMention.Visibility = Visibility.Visible);
            }
            if (_chat.UnreadReactionCount > 0)
            {
                await DispatcherQueue.EnqueueAsync(() => ButtonUnreadReaction.Visibility = Visibility.Visible);
            }
            
            switch (_chat.Type)
            {
                case TdApi.ChatType.ChatTypePrivate typePrivate:
                    var user = await _client.GetUserAsync(userId: typePrivate.UserId)
                        .WaitAsync(new CancellationToken());
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        ChatMembers.Text = user.Status switch
                        {
                            TdApi.UserStatus.UserStatusOnline => "Online",
                            TdApi.UserStatus.UserStatusOffline => "Offline",
                            TdApi.UserStatus.UserStatusRecently => "Last seen recently",
                            TdApi.UserStatus.UserStatusLastWeek => "Last week",
                            TdApi.UserStatus.UserStatusLastMonth => "Last month",
                            TdApi.UserStatus.UserStatusEmpty => "A long time",
                            _ => "Unknown"
                        };
                    });
                    break;
                case TdApi.ChatType.ChatTypeBasicGroup typeBasicGroup:
                    var basicGroupInfo = await _client.GetBasicGroupFullInfoAsync(
                        basicGroupId: typeBasicGroup.BasicGroupId);
                    await DispatcherQueue.EnqueueAsync(() => ChatMembers.Text = basicGroupInfo.Members.Length + " members");
                    break;
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                    try
                    {
                        var supergroup = await _client.GetSupergroupAsync(
                            supergroupId: typeSupergroup.SupergroupId);
                        var supergroupInfo = await _client.GetSupergroupFullInfoAsync(
                            supergroupId: typeSupergroup.SupergroupId);
                        _linkedChatId = supergroupInfo.LinkedChatId;

                        if (supergroup.IsChannel)
                        {
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                UserActionsPanel.Visibility = supergroup.Status switch
                                {
                                    TdApi.ChatMemberStatus.ChatMemberStatusCreator => Visibility.Visible,
                                    TdApi.ChatMemberStatus.ChatMemberStatusAdministrator => Visibility.Visible,
                                    TdApi.ChatMemberStatus.ChatMemberStatusMember => Visibility.Collapsed,
                                    TdApi.ChatMemberStatus.ChatMemberStatusBanned => Visibility.Collapsed,
                                    TdApi.ChatMemberStatus.ChatMemberStatusRestricted => Visibility.Collapsed,
                                    TdApi.ChatMemberStatus.ChatMemberStatusLeft => Visibility.Collapsed,
                                    _ => Visibility.Collapsed
                                };

                                ButtonFastAction.Visibility = Visibility.Visible;
                            });
                        }
                        else
                        {
                            await DispatcherQueue.EnqueueAsync(() => ButtonFastAction.Visibility = Visibility.Collapsed);
                        }

                        await DispatcherQueue.EnqueueAsync(() => ChatMembers.Text = supergroupInfo.MemberCount + " members");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        throw;
                    }

                    break;
            }
            
            var messages = await GetMessagesAsync(_chatId, _isForum);
            await GenerateMessageByType(messages);
            if (_chat.UnreadCount == 0) MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };

            var settings = SettingsService.LoadSettings();
            IconMedia.Glyph = settings.StartMediaPage switch
            {
                "Emojis" => "&#xE76E;",
                "Stickers" => "&#xF4AA;",
                "GIFs" => "&#xF4A9;",
                _ => string.Empty
            };
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
            
        // string channelBottomButtonValue = (string)_localSettings.Values["ChannelBottomButton"];
        //
        // switch (channelBottomButtonValue)
        // {
        //     case "Discuss":
        //     {
        //         ButtonFastAction.Content = "Discuss";
        //         break;
        //     }
        //     case "Mute/Unmute":
        //     {
        //         ButtonFastAction.Content = "Mute/Unmute";
        //         break;
        //     }
        //     case "Hide":
        //     {
        //         ButtonFastAction.Content = "Hide";
        //         break;
        //     }
        // }
    }

    public async Task<List<TdApi.Message>> GetMessagesAsync(long chatId = 0, bool isMessageThread = false, long lastMessageId = 0, int offset = 0)
    {
        try
        {
            if (chatId == 0) return new List<TdApi.Message>();
            
            var chat = await _client.GetChatAsync(chatId);
            int totalMessagesLoaded = 0;
            const int maxMessagesToLoad = 50;

            List<TdApi.Message> allMessages = new();

            while (totalMessagesLoaded < maxMessagesToLoad && chat != null)
            {
                TdApi.Messages messages;
                if (!isMessageThread)
                {
                    messages = await _client.ExecuteAsync(new TdApi.GetChatHistory
                    {
                        ChatId = chatId,
                        FromMessageId = lastMessageId,
                        Limit = Math.Min(maxMessagesToLoad - totalMessagesLoaded, 100),
                        OnlyLocal = _hasInternetConnection
                    });
                    return messages.Messages_.Reverse().ToList();
                }
                else
                {
                    messages = await _client.ExecuteAsync(new TdApi.GetMessageThreadHistory
                    {
                        ChatId = chatId,
                        MessageId = lastMessageId,
                        Limit = Math.Min(maxMessagesToLoad - totalMessagesLoaded, 100)
                    });
                    return messages.Messages_.Reverse().ToList();
                }

                if (messages.Messages_ == null || !messages.Messages_.Any())
                {
                    break;
                }

                foreach (var message in messages.Messages_)
                {
                    if (!allMessages.Contains(message))
                    {
                        allMessages.Add(message);
                    }
                }

                totalMessagesLoaded += messages.Messages_.Length;
                offset += messages.Messages_.Length;
            }

            return allMessages;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }
    
    public async Task GenerateMessageByType(List<TdApi.Message> messages, bool clear = false)
    {
        List<TdApi.Message> addedMessages = [];
        
        await DispatcherQueue.EnqueueAsync(async () =>
        {
            if (clear) MessagesList.Children.Clear();
            
            foreach (var message in messages.Where(message => !addedMessages.Contains(message)))
            {
                var messageEntry = new ChatMessage(this, _replyService);
                messageEntry._messageService = _messageService;
                await messageEntry.UpdateMessage(message, null);
                MessagesList.Children.Add(messageEntry);
                
                // switch (message.Content)
                // {
                //     case TdApi.MessageContent.MessageText:
                //     {
                //         var textMessage = new ChatMessage(this, _replyService);
                //         textMessage._messageService = _messageService;
                //         await textMessage.UpdateMessage(message, null);
                //         MessagesList.Children.Add(textMessage);
                //         break;
                //     }
                //     case TdApi.MessageContent.MessageChatChangeTitle or TdApi.MessageContent.MessagePinMessage 
                //         or TdApi.MessageContent.MessageGiftedPremium
                //         or TdApi.MessageContent.MessageGameScore or TdApi.MessageContent.MessageChatBoost 
                //         or TdApi.MessageContent.MessageUnsupported:
                //     {
                //         var changeTitleMessage = new ChatServiceMessage();
                //         changeTitleMessage.UpdateMessage(message);
                //         MessagesList.Children.Add(changeTitleMessage);
                //         break;
                //     }
                //     case TdApi.MessageContent.MessageSticker or TdApi.MessageContent.MessageAnimation:
                //     {
                //         var stickerMessage = new ChatMessage(this, _replyService);
                //         await stickerMessage.UpdateMessage(message, null);
                //         MessagesList.Children.Add(stickerMessage);
                //         break;
                //     }
                //     case TdApi.MessageContent.MessagePhoto:
                //     {
                //         // if (message.MediaAlbumId != 0)
                //         // {
                //         //     _mediaAlbumId = message.MediaAlbumId;
                //         //     _mediaAlbum.Add(message);
                //         // }
                //     
                //         var photoMessage = new ChatMessage(this, _replyService);
                //         await photoMessage.UpdateMessage(message, null);
                //         MessagesList.Children.Add(photoMessage);
                //         break;
                //     }
                // }
                addedMessages.Add(message);
            }
        });
    }
        
    private async void SendMessage_OnClick(object sender, RoutedEventArgs e)
    {
        if (UserMessageInput.Text.Length <= 0) return;
        if (_replyService.GetReplyMessageId() == 0)
        {
            await _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = _chatId,
                InputMessageContent = new TdApi.InputMessageContent.InputMessageText
                {
                    Text = new TdApi.FormattedText {Text = UserMessageInput.Text},
                }
            });
        }
        else
        {
            _replyService.ReplyOnMessage(_chatId, UserMessageInput.Text);
            PrepareDraftMessage(null);
        }

        //MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);
        //MessagesScrollViewer.ChangeView(0, 1, 1);
        UserMessageInput.Text = string.Empty;
    }

    private void MoreActions_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenu.ShowAt(MoreActions);
    }

    private async void PrepareDraftMessage(TdApi.DraftMessage draftMessage)
    {
        if (draftMessage.InputMessageText == null) return;
        await DispatcherQueue.EnqueueAsync(() => UserMessageInput.Text = draftMessage.InputMessageText switch
        {
            TdApi.InputMessageContent.InputMessageText text => text.Text.Text,
            _ => string.Empty
        });

        if (draftMessage.ReplyTo == null) return;
        switch (draftMessage.ReplyTo)
        {
            case TdApi.InputMessageReplyTo.InputMessageReplyToExternalMessage inputMessageReplyToExternalMessage:
                await UpdateReplyInfo(inputTextQuote: inputMessageReplyToExternalMessage);
                _replyService.SelectMessageForReply(inputMessageReplyToExternalMessage.MessageId);
                break;
            case TdApi.InputMessageReplyTo.InputMessageReplyToMessage replyToMessage:
                await UpdateReplyInfo(inputReplyTo: replyToMessage);
                _replyService.SelectMessageForReply(replyToMessage.MessageId);
                break;
            case TdApi.InputMessageReplyTo.InputMessageReplyToStory inputMessageReplyToStory:
                await UpdateReplyInfo(storyId: inputMessageReplyToStory.StoryId);
                break;
        }
    }
    
    public async Task UpdateReplyInfo(TdApi.Message messageReply = null,
        TdApi.InputMessageReplyTo.InputMessageReplyToMessage inputReplyTo = null,
        TdApi.InputMessageReplyTo.InputMessageReplyToExternalMessage inputTextQuote = null, int storyId = 0)
    {
        string senderName = string.Empty;
        string messageText = string.Empty;
        TdApi.Message message = null;
        
        if (messageReply != null)
        {
            message = messageReply;
            senderName = await UserService.GetSenderName(messageReply);
            messageText = await MessageService.GetTextMessageContent(messageReply);
        }
        else if (inputTextQuote != null)
        {
            message = await _client.GetMessageAsync(inputTextQuote.MessageId);
            var chat = await _client.GetChatAsync(message.ChatId);
            senderName = await UserService.GetSenderName(message);
            senderName = chat.Title + "=>" + senderName;
            messageText = inputTextQuote.Quote.Text.Text;
        }
        else if (inputReplyTo != null)
        {
            message = await _client.GetMessageAsync(_chatId, inputReplyTo.MessageId);
            senderName = await UserService.GetSenderName(message);
            messageText = inputReplyTo.Quote != null 
                ? inputReplyTo.Quote.Text.Text 
                : await MessageService.GetTextMessageContent(message);
        }
        
        messageText = Regex.Replace(messageText, @"\s+", " ");
        messageText = messageText.Trim();

        await DispatcherQueue.EnqueueAsync(async () =>
        {
            if (message != null)
            {
                switch (message.Content)
                {
                    case TdApi.MessageContent.MessageAnimation messageAnimation:
                        if (messageAnimation.Animation.Minithumbnail != null)
                        {
                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                ImageReplyMedia.Source =
                                    await MediaService.GetImageFromByteArray(messageAnimation.Animation.Minithumbnail.Data);
                                ImageReplyMedia.Visibility = Visibility.Visible;
                            });
                        }
                        break;
                    case TdApi.MessageContent.MessagePhoto messagePhoto:
                        if (messagePhoto.Photo.Minithumbnail != null)
                        {
                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                ImageReplyMedia.Source =
                                    await MediaService.GetImageFromByteArray(messagePhoto.Photo.Minithumbnail.Data);
                                ImageReplyMedia.Visibility = Visibility.Visible;
                            });
                        }
                        break;
                    case TdApi.MessageContent.MessageVideo messageVideo:
                        if (messageVideo.Video.Minithumbnail != null)
                        {
                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                ImageReplyMedia.Source =
                                    await MediaService.GetImageFromByteArray(messageVideo.Video.Minithumbnail.Data);
                                ImageReplyMedia.Visibility = Visibility.Visible;
                            });
                        }
                        break;
                    case TdApi.MessageContent.MessageVideoNote messageVideoNote:
                        if (messageVideoNote.VideoNote.Minithumbnail != null)
                        {
                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                ImageReplyMedia.Source =
                                    await MediaService.GetImageFromByteArray(messageVideoNote.VideoNote.Minithumbnail
                                        .Data);
                                ImageReplyMedia.Visibility = Visibility.Visible;
                            });
                        }
                        break;
                    default:
                        await DispatcherQueue.EnqueueAsync(() => ImageReplyMedia.Visibility = Visibility.Collapsed);
                        break;
                }
            }
            
            TextBlockReplyName.Text = $"Reply to {senderName}";
            TextBlockReplyContent.Text = messageText;
            BorderReply.Visibility = Visibility.Visible;
            RowUserActions.Height = new GridLength(100);
            GridUserActions.Height = 100;
        });
    }
        
    public void CloseChat()
    {
        if (MessagesList.Children.Count > 0) MessagesList.Children.Clear();
        _client.CloseChatAsync(_chatId);
        _ChatsView.CloseChat();
    }
        
    private void ContextMenuNotifications_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ContextMenuViewGroupInfo_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ContextMenuToBeginning_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ContextMenuManageGroup_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ContextMenuBoostGroup_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ContextMenuCreatePoll_OnClick(object sender, RoutedEventArgs e)
    {
        CreatePoll.ShowAsync();
    }

    private void ContextMenuReport_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ExecuteAsync(new TdApi.ReportChat
        {
            ChatId = _chatId
        });
    }

    private void ContextMenuClearHistory_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ExecuteAsync(new TdApi.DeleteChatHistory
        {
            ChatId = _chatId, RemoveFromChatList = false, Revoke = true
        });
    }

    private void ContextMenuLeaveGroup_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void CreatePoll_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (PollQuestion.Text == string.Empty) return;
            
        try
        {
            _pollOptionsList.Add(new TdApi.FormattedText {Text = DefaultPollOption.Text});
            _pollOptionsList.Add(new TdApi.FormattedText {Text = DefaultSecondaryPollOption.Text});
                
            foreach (var pollOption in AdditionalPollOptions.Children.OfType<TextBox>())
            {
                _pollOptionsList.Add(new TdApi.FormattedText {Text = pollOption.Text});
            }
                
            _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = _chatId,
                InputMessageContent = new TdApi.InputMessageContent.InputMessagePoll
                {
                    Type = QuizMode.IsChecked.Value ? new TdApi.PollType.PollTypeQuiz
                    {
                        CorrectOptionId = Convert.ToInt32(TextBoxCorrectAnswer.Text) - 1
                    } : new TdApi.PollType.PollTypeRegular
                    {
                        AllowMultipleAnswers = MultipleAnswers.IsChecked.Value
                    },
                    Question = new TdApi.FormattedText { Text = PollQuestion.Text },
                    Options = _pollOptionsList.ToArray(),
                    IsClosed = false,
                    IsAnonymous = AnonymousVoting.IsChecked.Value,
                }
            });
                
            _pollOptionsList.Clear();
            AdditionalPollOptions.Children.Clear();
            DefaultPollOption.Text = string.Empty;
            DefaultSecondaryPollOption.Text = string.Empty;
        }
        catch (TdException e)
        {
            CreatePoll.Hide();
            TextBlockException.Text = e.Message;
            ExceptionDialog.ShowAsync();
            throw;
        }
    }

    private void CreatePoll_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
        if (_pollOptionsCount >= 10) return;

        DispatcherQueue.EnqueueAsync(() => {
            var newPollOption = new TextBox
            {
                PlaceholderText = "Add an option",
                Margin = new Thickness(0, 0, 0, 4)
            };
                
            _pollOptionsCount += 1;
            AdditionalPollOptions.Children.Add(newPollOption);
        });
    }

    private void SearchMessages_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void CreateVideoCall_OnClick(object sender, RoutedEventArgs e)
    {
        var videoCall = new VoiceCall();
        videoCall.Activate();
    }

    private void Chat_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Enter when UserMessageInput.Text != "":
                SendMessage_OnClick(sender, null);
                break;
            case VirtualKey.Escape:
                if (_isProfileOpened)
                {
                    Profile.Hide();
                }
                else
                {
                    CloseChat();
                }
                break;
        }
    }

    private void ButtonAttachMedia_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenuMedia.ShowAt(ButtonAttachMedia);
    }

    private void ContextMenuPhotoOrVideo_OnClick(object sender, RoutedEventArgs e)
    {
        // SendMediaMessage.ShowAsync();
    }

    private void ContextMenuFile_OnClick(object sender, RoutedEventArgs e)
    {
        // SendFileMessage.ShowAsync();
    }

    private void ContextMenuPoll_OnClick(object sender, RoutedEventArgs e)
    {
        CreatePoll.ShowAsync();
    }

    private void QuizMode_OnChecked(object sender, RoutedEventArgs e)
    {
        TextBoxCorrectAnswer.IsEnabled = QuizMode.IsChecked.Value;
        MultipleAnswers.IsEnabled = !QuizMode.IsChecked.Value;
    }

    private void TopBar_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
        {
            Profile.ShowAsync();
        }
    }

    private async void Profile_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        switch (_chat.Type)
        {
            case TdApi.ChatType.ChatTypePrivate typePrivate:
            {
                var user = _client.GetUserAsync(typePrivate.UserId).Result;
                var userFullInfo = _client.GetUserFullInfoAsync(typePrivate.UserId).Result;
                    
                Profile.Title = user.FirstName + " " + user.LastName;
                TextBlockName.Text = user.FirstName + " " + user.LastName;
                CardUsername.Header = "@" + user.Usernames.ActiveUsernames[0];
                CardBio.Header = userFullInfo.Bio.Text;
                CardUserId.Header = user.Id;
                    
                GroupInfo.Visibility = Visibility.Collapsed;

                if (user.ProfilePhoto != null)
                {
                    if (user.ProfilePhoto.Small.Local.Path != string.Empty)
                    {
                        ProfilePicture.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Small.Local.Path));
                    }
                    else
                    {
                        await _client.ExecuteAsync(new TdApi.DownloadFile
                        {
                            FileId = user.ProfilePhoto.Small.Id,
                            Priority = 1
                        });
                    }
                }
                else
                {
                    ProfilePicture.DisplayName = user.FirstName + " " + user.LastName;
                }

                if (userFullInfo.PersonalChatId != 0)
                {
                    var personalChat = _client.GetChatAsync(userFullInfo.PersonalChatId).Result;
                        
                    TextBlockChannelName.Text = personalChat.Title;
                    //TextBlockLastMessage.Text = personalChat.LastMessage.Content.ToString();

                    switch (personalChat.Type)
                    {
                        case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                        {
                            var chatFullInfo = _client.GetSupergroupFullInfoAsync(typeSupergroup.SupergroupId).Result;
                            TextBlockMembers.Text = $"{chatFullInfo.MemberCount} members";
                            break;
                        }
                    }
                        
                    if (personalChat.Photo != null)
                    {
                        if (personalChat.Photo.Small.Local.Path != string.Empty)
                        {
                            ChannelPicture.ProfilePicture = new BitmapImage(new Uri(personalChat.Photo.Small.Local.Path));
                        }
                        else
                        {
                            var photo = await _client.ExecuteAsync(new TdApi.DownloadFile
                            {
                                FileId = personalChat.Photo.Small.Id,
                                Priority = 1
                            });
                            ChannelPicture.ProfilePicture = new BitmapImage(new Uri(photo.Local.Path));
                        }
                    }
                    else
                    {
                        ChannelPicture.DisplayName = personalChat.Title;
                    }
                        
                    ChannelInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    SeparatorGroup.Visibility = Visibility.Collapsed;
                    ChannelInfo.Visibility = Visibility.Collapsed;
                }

                break;
            }
            case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
            {
                var fullInfo = _client.GetSupergroupFullInfoAsync(typeSupergroup.SupergroupId).Result;
                    
                Profile.Title = _chat.Title;
                TextBlockName.Text = _chat.Title;
                TextBlockMembersOrStatus.Text = $"{fullInfo.MemberCount} members";
                CardChatId.Header = _chat.Id;
                    
                if (fullInfo.Description != string.Empty)
                {
                    CardDescription.Header = fullInfo.Description;
                    CardDescription.Visibility = Visibility.Visible;
                }
                else
                {
                    CardDescription.Visibility = Visibility.Collapsed;
                }
                    
                if (fullInfo.InviteLink != null)
                {
                    if (fullInfo.InviteLink.InviteLink != string.Empty)
                    {
                        CardLink.Header = fullInfo.InviteLink.InviteLink;
                        CardLink.Description = "Link";
                        CardLink.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CardLink.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    CardLink.Visibility = Visibility.Collapsed;
                }
                    
                SeparatorMain.Visibility = Visibility.Collapsed;
                UserInfo.Visibility = Visibility.Collapsed;
                ChannelInfo.Visibility = Visibility.Collapsed;
                    
                if (_chat.Photo != null)
                {
                    if (_chat.Photo.Small.Local.Path != string.Empty)
                    {
                        ProfilePicture.ProfilePicture = new BitmapImage(new Uri(_chat.Photo.Small.Local.Path));
                    }
                    else
                    {
                        await _client.ExecuteAsync(new TdApi.DownloadFile
                        {
                            FileId = _chat.Photo.Small.Id,
                            Priority = 1
                        });
                    }
                }
                else
                {
                    ProfilePicture.DisplayName = _chat.Title;
                }
                    
                break;
            }
            case TdApi.ChatType.ChatTypeBasicGroup typeGroup:
            {
                var fullInfo = _client.GetBasicGroupFullInfoAsync(typeGroup.BasicGroupId).Result;
                    
                Profile.Title = _chat.Title;
                TextBlockName.Text = _chat.Title;
                TextBlockMembersOrStatus.Text = $"{fullInfo.Members.Length} members";
                CardChatId.Header = _chat.Id;

                if (fullInfo.Description != string.Empty)
                {
                    CardDescription.Header = fullInfo.Description;
                    CardDescription.Visibility = Visibility.Visible;
                }
                else
                {
                    CardDescription.Visibility = Visibility.Collapsed;
                }
                    
                if (fullInfo.InviteLink != null)
                {
                    if (fullInfo.InviteLink.InviteLink != string.Empty)
                    {
                        CardLink.Header = fullInfo.InviteLink.InviteLink;
                        CardLink.Description = "Link";
                        CardLink.Visibility = Visibility.Visible;
                    }
                    if (fullInfo.InviteLink.Name != string.Empty)
                    {
                        CardLink.Header = fullInfo.InviteLink.Name;
                        CardLink.Description = "Link";
                        CardLink.Visibility = Visibility.Visible;
                    }
                }
                    
                SeparatorMain.Visibility = Visibility.Collapsed;
                UserInfo.Visibility = Visibility.Collapsed;
                ChannelInfo.Visibility = Visibility.Collapsed;
                    
                if (_chat.Photo != null)
                {
                    if (_chat.Photo.Small.Local.Path != string.Empty)
                    {
                        ProfilePicture.ProfilePicture = new BitmapImage(new Uri(_chat.Photo.Small.Local.Path));
                    }
                    else
                    {
                        await _client.ExecuteAsync(new TdApi.DownloadFile
                        {
                            FileId = _chat.Photo.Small.Id,
                            Priority = 1
                        });
                    }
                }
                else
                {
                    ProfilePicture.DisplayName = _chat.Title;
                }
                    
                break;
            }
        }
    }

    private void Stickers_OnClick(object sender, RoutedEventArgs e)
    {
        _mediaMenuOpened = !_mediaMenuOpened;
        UpdateMediaLength();
        UpdateMediaVisibility();
    }
    
    private void PanelMedia_OnLoaded(object sender, RoutedEventArgs e)
    {
        var settings = SettingsService.LoadSettings();
        int index = settings.StartMediaPage switch
        {
            "Emojis" => 0,
            "Stickers" => 1,
            "GIFs" => 2,
            _ => 0,
        };
        
        SelectMediaPage(index);
    }

    private void SelectMediaPage(int index)
    {
        _currentMediaPage = index;
        
        switch (index)
        {
            case 0: // Emojis
                BarItemEmojis.IsSelected = false;
                BarItemStickers.IsSelected = true;
                BarItemAnimations.IsSelected = true;
                break;
            case 1: // Stickers
                GenerateStickers();
                BarItemEmojis.IsSelected = true;
                BarItemStickers.IsSelected = false;
                BarItemAnimations.IsSelected = true;
                break;
            case 2: // GIFs
                GenerateAnimations();
                BarItemEmojis.IsSelected = true;
                BarItemStickers.IsSelected = true;
                BarItemAnimations.IsSelected = false;
                break;
        }
    }

    private async void GenerateStickers()
    {
        PanelMedia.Children.Clear();
        
        var favoriteStickers = await _client.ExecuteAsync(new TdApi.GetFavoriteStickers());
        var recentStickers = await _client.ExecuteAsync(new TdApi.GetRecentStickers { IsAttached = false });
        var stickerSets = await _client.ExecuteAsync(new TdApi.GetInstalledStickerSets());
        
        if (stickerSets.Sets.Length > 0)
        {
            foreach (var set in stickerSets.Sets)
            {
                var foundedSet = await _client.ExecuteAsync(new TdApi.GetStickerSet { SetId = set.Id });
                _stickerSets.Add(foundedSet);
            }

            _alreadyAddedStickerSets.Add(_stickerSets[0]);
        }

        if (favoriteStickers.Stickers_.Length > 0)
        {
            int favoriteRow = 0;
            int favoriteCol = 0;
            
            TextBlock textBlockPinnedStickers = new()
            {
                Text = "Favorite stickers",
                FontSize = 12,
            };
            PanelMedia.Children.Add(textBlockPinnedStickers);
            
            Grid favoriteStickersGrid = new();
            PanelMedia.Children.Add(favoriteStickersGrid);
            
            foreach (var sticker in favoriteStickers.Stickers_)
            {
                var newSticker = new Sticker(sticker);

                if (favoriteCol == 5)
                {
                    favoriteCol = 0;
                    favoriteRow++;
                    if (favoriteRow >= favoriteStickersGrid.RowDefinitions.Count)
                    {
                        await DispatcherQueue.EnqueueAsync(() =>
                            favoriteStickersGrid.RowDefinitions.Add(new RowDefinition { }));
                    }
                }

                if (favoriteCol >= favoriteStickersGrid.ColumnDefinitions.Count)
                {
                    await DispatcherQueue.EnqueueAsync(() => favoriteStickersGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(64, GridUnitType.Auto) }));
                }

                await DispatcherQueue.EnqueueAsync(() =>
                {
                    favoriteStickersGrid.Children.Add(newSticker);
                    Grid.SetRow(newSticker, favoriteRow);
                    Grid.SetColumn(newSticker, favoriteCol);
                });
                favoriteCol++;
            }
        }

        if (recentStickers.Stickers_.Length > 0)
        {
            int recentRow = 0;
            int recentCol = 0;
        
            TextBlock textBlockRecentStickers = new()
            {
                Text = "Recent stickers",
                FontSize = 12,
            };
            PanelMedia.Children.Add(textBlockRecentStickers);
        
            Grid recentStickersGrid = new();
            PanelMedia.Children.Add(recentStickersGrid);
            
            foreach (var sticker in recentStickers.Stickers_)
            {
                var newSticker = new Sticker(sticker);

                if (recentCol == 5)
                {
                    recentCol = 0;
                    recentRow++;
                    if (recentRow >= recentStickersGrid.RowDefinitions.Count)
                    {
                        await DispatcherQueue.EnqueueAsync(() =>
                            recentStickersGrid.RowDefinitions.Add(new RowDefinition { }));
                    }
                }

                if (recentCol >= recentStickersGrid.ColumnDefinitions.Count)
                {
                    await DispatcherQueue.EnqueueAsync(() => recentStickersGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(64, GridUnitType.Auto) }));
                }

                await DispatcherQueue.EnqueueAsync(() =>
                {
                    recentStickersGrid.Children.Add(newSticker);
                    Grid.SetRow(newSticker, recentRow);
                    Grid.SetColumn(newSticker, recentCol);
                });
                recentCol++;
            }
        }

        if (stickerSets.Sets.Length > 0)
        {
            int setRow = 0;
            int setCol = 0;
            TdApi.StickerSet stickerSet = await _client.GetStickerSetAsync(stickerSets.Sets[0].Id);
            
            TextBlock textBlockRecentStickers = new()
            {
                Text = stickerSets.Sets[0].Title,
                FontSize = 12,
            };
            PanelMedia.Children.Add(textBlockRecentStickers);
        
            Grid recentStickersGrid = new();
            PanelMedia.Children.Add(recentStickersGrid);
            
            foreach (var sticker in stickerSet.Stickers)
            {
                var newSticker = new Sticker(sticker);

                if (setCol == 5)
                {
                    setCol = 0;
                    setRow++;
                    if (setRow >= recentStickersGrid.RowDefinitions.Count)
                    {
                        await DispatcherQueue.EnqueueAsync(() =>
                            recentStickersGrid.RowDefinitions.Add(new RowDefinition { }));
                    }
                }

                if (setCol >= recentStickersGrid.ColumnDefinitions.Count)
                {
                    await DispatcherQueue.EnqueueAsync(() => recentStickersGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(64, GridUnitType.Auto) }));
                }

                await DispatcherQueue.EnqueueAsync(() =>
                {
                    recentStickersGrid.Children.Add(newSticker);
                    Grid.SetRow(newSticker, setRow);
                    Grid.SetColumn(newSticker, setCol);
                });
                setCol++;
            }

            _currentStickerSet++;
        }
    }

    private async void GenerateAnimations()
    {
        PanelMedia.Children.Clear();
        
        var animations = await _client.GetSavedAnimationsAsync();
        int savedRow = 0;
        int savedCol = 0;
        
        Grid savedAnimationsGrid = new();
        PanelMedia.Children.Add(savedAnimationsGrid);
        
        foreach (var animation in animations.Animations_)
        {
            var newAnimation = new Animation(animation);

            if (savedCol == 3)
            {
                savedCol = 0;
                savedRow++;
                if (savedRow >= savedAnimationsGrid.RowDefinitions.Count)
                {
                    await DispatcherQueue.EnqueueAsync(() => savedAnimationsGrid.RowDefinitions.Add(new RowDefinition()));
                }
            }
            
            if (savedCol >= savedAnimationsGrid.ColumnDefinitions.Count)
            {
                await DispatcherQueue.EnqueueAsync(() => savedAnimationsGrid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(128, GridUnitType.Auto) }));
            }

            await DispatcherQueue.EnqueueAsync(() =>
            {
                savedAnimationsGrid.Children.Add(newAnimation);
                Grid.SetRow(newAnimation, savedRow);
                Grid.SetColumn(newAnimation, savedCol);
            });
            savedCol++;
        }
    }
    
    private async void ScrollViewer_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_currentMediaPage != 1) return;
        if (!e.IsIntermediate)
        {
            var scrollViewer = (ScrollViewer)sender;
            
            double verticalOffset = scrollViewer.VerticalOffset;
            double maxVerticalOffset = scrollViewer.ExtentHeight - scrollViewer.ViewportHeight;

            if (verticalOffset >= maxVerticalOffset)
            {
                int startIndex = _stickerSets.Count;
                int endIndex = 0;
                for (int i = _currentStickerSet; i <= _currentStickerSet + 1; i++)
                {
                    if (_alreadyAddedStickerSets.Contains(_stickerSets[i])) continue;
                    _alreadyAddedStickerSets.Add(_stickerSets[i]);
                    
                    endIndex = i;
                    int setRow = 0;
                    int setCol = 0;
                    
                    TextBlock textBlockRecentStickers = new()
                    {
                        Text = _stickerSets[i].Title,
                        FontSize = 12,
                    };
                    PanelMedia.Children.Add(textBlockRecentStickers);
        
                    Grid recentStickersGrid = new();
                    PanelMedia.Children.Add(recentStickersGrid);
                    
                    foreach (var sticker in _stickerSets[i].Stickers)
                    {
                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            var newSticker = new Sticker(sticker);

                            if (setCol == 5)
                            {
                                setCol = 0;
                                setRow++;
                                if (setRow >= recentStickersGrid.RowDefinitions.Count)
                                {
                                    recentStickersGrid.RowDefinitions.Add(new RowDefinition { });
                                }
                            }

                            if (setCol >= recentStickersGrid.ColumnDefinitions.Count)
                            {
                                recentStickersGrid.ColumnDefinitions.Add(new ColumnDefinition
                                    { Width = new GridLength(64, GridUnitType.Auto) });
                            }

                            recentStickersGrid.Children.Add(newSticker);
                            Grid.SetRow(newSticker, setRow);
                            Grid.SetColumn(newSticker, setCol);
                        });
                        setCol++;
                    }
                }
                _currentStickerSet = endIndex;
            }
        }
    }

    private void ButtonReplyClose_OnClick(object sender, RoutedEventArgs e)
    {
        BorderReply.Visibility = Visibility.Collapsed;
        RowUserActions.Height = new GridLength(45);
        GridUserActions.Height = 45;
    }

    private void ButtonUnreadMessages_OnClick(object sender, RoutedEventArgs e)
    {
        MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);
    }
}