using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using sakuragram.Views.Calls;
using sakuragram.Views.Chats.Messages;
using TdLib;
using DispatcherQueuePriority = Microsoft.UI.Dispatching.DispatcherQueuePriority;

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

    #region LoadProperties

    private bool _isNeedLoadFastActions = true;

    #endregion
        
    private ReplyService _replyService;
    private MessageService _messageService;

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
        
    }

    private Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateNewMessage updateNewMessage:
            {
                if (updateNewMessage.Message.ChatId == _chatId)
                {
                    DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await GenerateMessageByType(updateNewMessage.Message);

                        // For debug
                        // var message = new ChatDebugMessage();
                        // MessagesList.Children.Add(message);
                        // message.UpdateMessage(updateNewMessage.Message);
                    });
                }
                break;
            }
            case TdApi.Update.UpdateChatAction updateChatAction:
            {
                if (updateChatAction.ChatId == _chatId)
                {
                    ChatTitle.DispatcherQueue.TryEnqueue(() => {});
                }
                break;
            }
            //     case TdApi.Update.UpdateChatTitle updateChatTitle:
            //     {
            //         ChatTitle.DispatcherQueue.TryEnqueue(() => ChatTitle.Text = updateChatTitle.Title);
            //         break;
            //     }
            //     case TdApi.Update.UpdateUserStatus updateUserStatus:
            //     {
            //         if (_chat.Type is TdApi.ChatType.ChatTypePrivate)
            //         {
            //             ChatMembers.DispatcherQueue.TryEnqueue(() =>
            //             {
            //                 var status = updateUserStatus.Status switch
            //                 {
            //                     TdApi.UserStatus.UserStatusOnline => "Online",
            //                     TdApi.UserStatus.UserStatusOffline => "Offline",
            //                     TdApi.UserStatus.UserStatusRecently => "Last seen recently",
            //                     TdApi.UserStatus.UserStatusLastWeek => "Last week",
            //                     TdApi.UserStatus.UserStatusLastMonth => "Last month",
            //                     TdApi.UserStatus.UserStatusEmpty => "Last seen a long time ago",
            //                     _ => "Unknown"
            //                 };
            //
            //                 ChatMembers.Text = status;
            //
            //                 if (_isProfileOpened)
            //                 {
            //                     TextBlockMembersOrStatus.Text = status;
            //                 }
            //             });
            //         }
            //         break;
            // }
            //     case TdApi.Update.UpdateChatOnlineMemberCount updateChatOnlineMemberCount:
            //     {
            //         if (_chat.Type is TdApi.ChatType.ChatTypeSupergroup or TdApi.ChatType.ChatTypeBasicGroup &&
            //             _chat.Permissions.CanSendBasicMessages)
            //         {
            //             _onlineMemberCount = updateChatOnlineMemberCount.OnlineMemberCount;
            //             ChatMembers.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            //             {
            //                 if (_onlineMemberCount > 0)
            //                 {
            //                     OnlineChatMembers.Text = $", online: {_onlineMemberCount}";
            //                     OnlineChatMembers.Visibility = Visibility.Visible;
            //                 }
            //                 else
            //                 {
            //                     OnlineChatMembers.Text = string.Empty;
            //                     OnlineChatMembers.Visibility = Visibility.Collapsed;
            //                 }
            //             });   
            //         }
            //         break;
            //     }
            //     case TdApi.Update.UpdateBasicGroup updateBasicGroup:
            //     {
            //         if (updateBasicGroup.BasicGroup.Id == _chatId)
            //         {
            //             _memberCount = updateBasicGroup.BasicGroup.MemberCount;
            //             ChatMembers.DispatcherQueue.TryEnqueue(UpdateChatMembersText);
            //         }
            //         break;
            //     }
            //     case TdApi.Update.UpdateSupergroup updateSupergroup:
            //     {
            //         if (updateSupergroup.Supergroup.Id == _chatId)
            //         {
            //             _memberCount = updateSupergroup.Supergroup.MemberCount;
            //             ChatMembers.DispatcherQueue.TryEnqueue(UpdateChatMembersText);
            //         }
            //         break;
            //     }
            //     case TdApi.Update.UpdateDeleteMessages updateDeleteMessages:
            //     {
            //         // if (updateDeleteMessages.ChatId == _chatId)
            //         // {
            //         //     var messages = MessagesList.Children;
            //         //     var messagesToRemove = messages.OfType<ChatMessage>();
            //         //     
            //         //     foreach (var messageId in updateDeleteMessages.MessageIds)
            //         //     {
            //         //         MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
            //         //         {
            //         //             foreach (var messageToRemove in messagesToRemove)
            //         //             {
            //         //                 if (messageToRemove._messageId == messageId)
            //         //                 {
            //         //                     MessagesList.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low,
            //         //                         () => MessagesList.Children.Remove(messageToRemove));
            //         //                 }
            //         //             }
            //         //         });
            //         //     }
            //         // }
            //         break;
            //     }
            //     case TdApi.Update.UpdateFile updateFile:
            //     {
            //         if (updateFile.File.Id == _backgroundId)
            //         {
            //             if (_background.Document.Document_.Local.Path != string.Empty)
            //             {
            //                 ThemeBackground.DispatcherQueue.TryEnqueue(() => 
            //                     ThemeBackground.ImageSource = new BitmapImage(
            //                         new Uri(_background.Document.Document_.Local.Path)
            //                     ));
            //             }
            //             else if (updateFile.File.Local.Path != string.Empty)
            //             {
            //                 ThemeBackground.DispatcherQueue.TryEnqueue(() => 
            //                     ThemeBackground.ImageSource = new BitmapImage(
            //                         new Uri(updateFile.File.Local.Path)
            //                     ));
            //             }
            //         }
            //         break;
            //     }
            //     case TdApi.Update.UpdateConnectionState updateConnectionState:
            //     {
            //         _hasInternetConnection = updateConnectionState.State switch
            //         {
            //             TdApi.ConnectionState.ConnectionStateReady => true,
            //             TdApi.ConnectionState.ConnectionStateConnecting => false,
            //             TdApi.ConnectionState.ConnectionStateUpdating => false,
            //             TdApi.ConnectionState.ConnectionStateConnectingToProxy => false,
            //             TdApi.ConnectionState.ConnectionStateWaitingForNetwork => false,
            //             _ => false
            //         };
            //         break;
            //     }
        }

        return Task.CompletedTask;
    }

    private async Task UpdateChat()
    {
        try
        {
            await _client.OpenChatAsync(_chatId);
            _chat = _client.GetChatAsync(_chatService._openedChatId).Result;
            await DispatcherQueue.EnqueueAsync(() => ChatTitle.Text = _isForum 
                ? _forumTopic.Info.Name
                : _chat.Title);
            
            if (_isForum) ChatPhoto.Visibility = Visibility.Collapsed;
            else await ChatPhoto.SetPhoto(null, _chat);

            if (_isForum)
            {
                if (_forumTopic.DraftMessage != null)
                {
                    UserMessageInput.Text = _forumTopic.DraftMessage.InputMessageText switch
                    {
                        TdApi.InputMessageContent.InputMessageText text => text.Text.Text,
                        _ => string.Empty
                    };
                }
            }
            else
            {
                if (_chat.DraftMessage != null)
                {
                    UserMessageInput.Text = _chat.DraftMessage.InputMessageText switch
                    {
                        TdApi.InputMessageContent.InputMessageText text => text.Text.Text,
                        _ => string.Empty
                    };
                }
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

            // var pinnedMessage = await _client.GetChatPinnedMessageAsync(_chat.Id);
            // if ()
            
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
                            TdApi.UserStatus.UserStatusRecently => "Recently",
                            TdApi.UserStatus.UserStatusLastWeek => "Last week",
                            TdApi.UserStatus.UserStatusLastMonth => "Last month",
                            TdApi.UserStatus.UserStatusEmpty => "A long time",
                            _ => "Unknown"
                        };
                    });
                    break;
                case TdApi.ChatType.ChatTypeBasicGroup typeBasicGroup:
                    var basicGroupInfo = await _client.GetBasicGroupFullInfoAsync(
                        basicGroupId: typeBasicGroup.BasicGroupId
                    ).ConfigureAwait(false);
                    await DispatcherQueue.EnqueueAsync(() => ChatMembers.Text = basicGroupInfo.Members.Length + " members");
                    break;
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                    try
                    {
                        var supergroup = await _client.GetSupergroupAsync(
                            supergroupId: typeSupergroup.SupergroupId).ConfigureAwait(false);
                        var supergroupInfo = await _client.GetSupergroupFullInfoAsync(
                            supergroupId: typeSupergroup.SupergroupId).ConfigureAwait(false);
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

            await _client.ExecuteAsync(new TdApi.OpenChat { ChatId = _chatService._openedChatId });
            if (_chat != null)
            {
                GetMessagesAsync(_chatId, _isForum);
                _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            throw;
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

    private async void GetMessagesAsync(long chatId, bool isForum)
    {
        try
        {
            var chat = await _client.GetChatAsync(chatId);
            int totalMessagesLoaded = 0;
            const int maxMessagesToLoad = 50;
            int offset = 0;

            List<TdApi.Message> allMessages = [];

            while (totalMessagesLoaded < maxMessagesToLoad && chat != null)
            {
                TdApi.Messages messages;
                if (!isForum)
                    messages = await _client.ExecuteAsync(new TdApi.GetChatHistory
                    {
                        ChatId = chatId,
                        FromMessageId = chat.LastMessage?.Id ?? 0,
                        Limit = Math.Min(maxMessagesToLoad - totalMessagesLoaded, 100),
                        Offset = 0,
                        OnlyLocal = _hasInternetConnection
                    });
                else
                    messages = await _client.ExecuteAsync(new TdApi.GetMessageThreadHistory
                    {
                        ChatId = chatId,
                        MessageId = _forumTopic.LastMessage.Id,
                        Offset = 0,
                        Limit = Math.Min(maxMessagesToLoad - totalMessagesLoaded, 100)
                    });

                if (messages.Messages_ == null || !messages.Messages_.Any())
                {
                    break;
                }

                allMessages.AddRange(messages.Messages_);
                totalMessagesLoaded += messages.Messages_.Length;
                offset += messages.Messages_.Length;
            }

            allMessages.Sort((m1, m2) => m1.Date.CompareTo(m2.Date));

            foreach (var message in allMessages)
            {
                await GenerateMessageByType(message);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }

    private long _mediaAlbumId;
    private List<TdApi.Message> _mediaAlbum;
    
    private async Task GenerateMessageByType(TdApi.Message message)
    {
        await DispatcherQueue.EnqueueAsync(() =>
        {
            switch (message.Content)
            {
                case TdApi.MessageContent.MessageText:
                {
                    var textMessage = new ChatMessage();
                    textMessage._messageService = _messageService;
                    textMessage.UpdateMessage(message, null);
                    MessagesList.Children.Add(textMessage);
                    break;
                }
                case TdApi.MessageContent.MessageChatChangeTitle or TdApi.MessageContent.MessagePinMessage 
                    or TdApi.MessageContent.MessageGiftedPremium
                    or TdApi.MessageContent.MessageGameScore or TdApi.MessageContent.MessageChatBoost 
                    or TdApi.MessageContent.MessageUnsupported:
                {
                    var changeTitleMessage = new ChatServiceMessage();
                    changeTitleMessage.UpdateMessage(message);
                    MessagesList.Children.Add(changeTitleMessage);
                    break;
                }
                case TdApi.MessageContent.MessageSticker or TdApi.MessageContent.MessageAnimation:
                {
                    var stickerMessage = new ChatMessage();
                    stickerMessage.UpdateMessage(message, null);
                    MessagesList.Children.Add(stickerMessage);
                    break;
                }
                case TdApi.MessageContent.MessageVideo:
                {
                    var videoMessage = new ChatVideoMessage();
                    videoMessage.UpdateMessage(message);
                    MessagesList.Children.Add(videoMessage);
                    break;
                }
                case TdApi.MessageContent.MessagePhoto:
                {
                    // if (message.MediaAlbumId != 0)
                    // {
                    //     _mediaAlbumId = message.MediaAlbumId;
                    //     _mediaAlbum.Add(message);
                    // }
                    
                    var photoMessage = new ChatMessage();
                    photoMessage.UpdateMessage(message, _mediaAlbum);
                    MessagesList.Children.Add(photoMessage);
                    break;
                }
                case TdApi.MessageContent.MessageDocument:
                {
                    var documentMessage = new ChatDocumentMessage();
                    documentMessage.UpdateMessage(message);
                    MessagesList.Children.Add(documentMessage);
                    break;
                }
                case TdApi.MessageContent.MessageVideoNote:
                {
                    var videoNoteMessage = new ChatVideoNoteMessage();
                    videoNoteMessage.UpdateMessage(message);
                    MessagesList.Children.Add(videoNoteMessage);
                    break;
                }
                case TdApi.MessageContent.MessageVoiceNote:
                {
                    var voiceNoteMessage = new ChatVoiceNoteMessage();
                    voiceNoteMessage.UpdateMessage(message);
                    MessagesList.Children.Add(voiceNoteMessage);
                    break;
                }
                case TdApi.MessageContent.MessagePoll:
                {
                    var pollMessage = new ChatPollMessage();
                    pollMessage.UpdateMessage(message);
                    MessagesList.Children.Add(pollMessage);
                    break;
                }
    #if DEBUG
                case TdApi.MessageContent.MessagePaidMedia:
                {
                    var paidMediaMessage = new ChatPaidMediaMessage();
                    paidMediaMessage.UpdateMessage(message);
                    MessagesList.Children.Add(paidMediaMessage);
                    break;
                }
    #endif
            }
            
            MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);
            MessagesScrollViewer.ChangeView(0, 1, 1);
        });
    }
        
    private async void SendMessage_OnClick(object sender, RoutedEventArgs e)
    {
            
        if (UserMessageInput.Text.Length <= 0) return;

        var text = _client.ExecuteAsync(new TdApi.ParseTextEntities
        {
            Text = UserMessageInput.Text,
            ParseMode = new TdApi.TextParseMode.TextParseModeMarkdown {Version = 0}
        }).Result;
        if (_replyService.GetReplyMessageId() == 0)
        {
            await _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = _chatId,
                InputMessageContent = new TdApi.InputMessageContent.InputMessageText
                {
                    Text = text,
                }
            });
        }
        else
        {
            _replyService.ReplyOnMessage(_chatId, _replyService.GetReplyMessageId(), UserMessageInput.Text);
        }

        //MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);
        //MessagesScrollViewer.ChangeView(0, 1, 1);
        UserMessageInput.Text = string.Empty;
    }

    private void MoreActions_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenu.ShowAt(MoreActions);
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

        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => {
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
        _ChatsView._mediaMenuOpened = !_ChatsView._mediaMenuOpened;
        _ChatsView.UpdateMediaLength();
        _ChatsView.UpdateMediaVisibility();
    }
}