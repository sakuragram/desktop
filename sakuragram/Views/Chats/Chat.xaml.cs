using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Controls.Messages;
using sakuragram.Services;
using sakuragram.Services.Chat;
using sakuragram.Services.Core;
using sakuragram.Views.Calls;
using sakuragram.Views.ChatList;
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
    private int _pollOptionsCount = 2;
    private bool _isProfileOpened = false;
    private bool _hasInternetConnection = true;
    private bool _isForum = false;
    public bool _mediaMenuOpened = false;

    #region LoadProperties

    private bool _isNeedLoadFastActions = true;

    #endregion
        
    private ReplyService _replyService = new();
    private MessageService _messageService;
    
    private int _chatActionRows = 0;
    private int _chatActionColumns = 0;
    
    private List<TdApi.StickerSet> _stickerSets = [];
    private List<TdApi.StickerSet> _alreadyAddedStickerSets = [];
    private int _currentStickerSet = -1;
    private int _currentMediaPage;
    
    private ChatMessage _penultimateChatMessage;
    
    private List<StorageFile> _mediaFiles = [];

    public Chat(long id, bool isForum, TdApi.ForumTopic forumTopic)
    {
        InitializeComponent();
        
        _chatId = id;
        _isForum = isForum;
        _forumTopic = forumTopic;
        _replyService = new ReplyService();
        _messageService = new MessageService();
        
        #if DEBUG
        {
            ButtonCreateVideoCall.IsEnabled = true;
        }
        #else
        {
            ButtonCreateVideoCall.IsEnabled = false;
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
                            // var message = new ChatDebugMessage();
                            // MessagesList.Children.Add(message);
                            // message.UpdateMessage(updateNewMessage.Message);
                        }
                        else
                        {
                            // await GenerateMessageByType([updateNewMessage.Message]);
                        }

                        // await DispatcherQueue.EnqueueAsync(() => 
                        //     MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight));
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
                        ChatMembers.Text = UserService.GetUserStatus(updateUserStatus.Status));
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
            var currentUser = await _client.GetMeAsync();

            bool isChannel = false;

            switch (_chat.Type)
            {
                case TdApi.ChatType.ChatTypePrivate typePrivate:
                    var user = await _client.GetUserAsync(typePrivate.UserId);
                    await DispatcherQueue.EnqueueAsync(async () =>
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
                        BorderForumTopics.Visibility = Visibility.Collapsed;
                        await OpenChatList();
                    });
                    break;
                case TdApi.ChatType.ChatTypeBasicGroup typeBasicGroup:
                    var basicGroupInfo = await _client.GetBasicGroupFullInfoAsync(
                        basicGroupId: typeBasicGroup.BasicGroupId);
                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        ChatMembers.Text = basicGroupInfo.Members.Length + " members";
                        BorderForumTopics.Visibility = Visibility.Collapsed;
                        await OpenChatList();
                    });
                    break;
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                    try
                    {
                        var supergroup = await _client.GetSupergroupAsync(typeSupergroup.SupergroupId);
                        var supergroupInfo = await _client.GetSupergroupFullInfoAsync(
                            supergroupId: typeSupergroup.SupergroupId);
                        _linkedChatId = supergroupInfo.LinkedChatId;

                        if (supergroup.IsChannel)
                        {
                            isChannel = typeSupergroup.IsChannel;
                            
                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                BorderForumTopics.Visibility = Visibility.Collapsed;
                                await OpenChatList();
                            });
                        }
                        else if (supergroup.IsForum)
                        {
                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                BorderForumTopics.Visibility = Visibility.Visible;
                                var topics = await _client.ExecuteAsync(new TdApi.GetForumTopics
                                {
                                    ChatId = _chat.Id,
                                    Limit = 100,
                                    OffsetMessageId = _chat.LastMessage.Id,
                                    OffsetMessageThreadId = _chat.LastMessage.MessageThreadId
                                });

                                foreach (var topic in topics.Topics)
                                {
                                    var selectorBarTopic = new SelectorBarItem
                                    {
                                        FontSize = 12,
                                        Tag = topic.Info.MessageThreadId,
                                        Text = topic.Info.Name,
                                    };
                                    selectorBarTopic.PointerPressed += async (_, _) => await OpenMessageThread((long)selectorBarTopic.Tag);
                                    SelectorBarTopics.Items.Add(selectorBarTopic);
                                }
                                
                                SelectorBarTopics.SelectedItem = SelectorBarTopics.Items[0];
                                await OpenMessageThread((long)SelectorBarTopics.SelectedItem.Tag);
                            });
                        }
                        else
                        {
                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                BorderPinnedMessage.Margin = new Thickness(0, 4, 0, 0);
                                BorderForumTopics.Visibility = Visibility.Collapsed;
                                await OpenChatList();
                            });
                        }

                        await DispatcherQueue.EnqueueAsync(() =>
                            ChatMembers.Text = supergroupInfo.MemberCount + " members");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        throw;
                    }

                    break;
            }

            try
            {
                var memberInfo = await _client.GetChatMemberAsync(_chat.Id,
                    new TdApi.MessageSender.MessageSenderUser { UserId = currentUser.Id });
                
                if (memberInfo != null)
                {
                    switch (memberInfo.Status)
                    {
                        case TdApi.ChatMemberStatus.ChatMemberStatusAdministrator administrator:

                            #region admin_rights

                            bool canManageChat = administrator.Rights.CanManageChat;
                            bool canPostMessages = administrator.Rights.CanPostMessages;
                            bool canDeleteMessages = administrator.Rights.CanDeleteMessages;
                            bool canEditMessages = administrator.Rights.CanEditMessages;
                            bool canChangeInfo = administrator.Rights.CanChangeInfo;
                            bool canPostStories = administrator.Rights.CanPostStories;
                            bool canDeleteStories = administrator.Rights.CanDeleteStories;
                            bool canEditStories = administrator.Rights.CanEditStories;
                            bool canInviteUsers = administrator.Rights.CanInviteUsers;
                            bool canManageTopics = administrator.Rights.CanManageTopics;
                            bool canPinMessages = administrator.Rights.CanPinMessages;
                            bool canPromoteMembers = administrator.Rights.CanPromoteMembers;
                            bool canRestrictMembers = administrator.Rights.CanRestrictMembers;
                            bool canManageVideoChats = administrator.Rights.CanManageVideoChats;
                            bool isAnonymous = administrator.Rights.IsAnonymous;

                            #endregion

                            if (canPostMessages && isChannel)
                            {
                                ButtonFastAction.Visibility = Visibility.Collapsed;
                                UserActionsPanel.Visibility = Visibility.Visible;
                            }
                            else if (!canPostMessages && isChannel)
                            {
                                ButtonFastAction.Visibility = Visibility.Visible;
                                UserActionsPanel.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                ButtonFastAction.Visibility = Visibility.Visible;
                                UserActionsPanel.Visibility = Visibility.Collapsed;
                            }
                            break;
                        case TdApi.ChatMemberStatus.ChatMemberStatusBanned banned:
                            var banDialog = new ContentDialog
                            {
                                Title = "You have been banned",
                                Content = "You have been banned from this group.",
                                CloseButtonText = "Close",
                                XamlRoot = this.XamlRoot
                            };
                            banDialog.ShowAsync();
                            break;
                        case TdApi.ChatMemberStatus.ChatMemberStatusCreator creator:
                            if (ButtonFastAction != null) ButtonFastAction.Visibility = Visibility.Collapsed;
                            UserActionsPanel.Visibility = Visibility.Visible;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            if (_isForum) ChatPhoto.Visibility = Visibility.Collapsed;
            else await ChatPhoto.InitializeProfilePhoto(chat: _chat, sizes: 30);

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

            if (_chat.ActionBar != null)
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    switch (_chat.ActionBar)
                    {
                        case TdApi.ChatActionBar.ChatActionBarAddContact:
                            Button addContact = new();
                            addContact.Content = "Add contact";
                            ActionBarSetGridPosition(addContact);
                            break;
                        case TdApi.ChatActionBar.ChatActionBarInviteMembers:
                            Button inviteMembers = new();
                            inviteMembers.Content = "Invite members";
                            ActionBarSetGridPosition(inviteMembers);
                            break;
                        case TdApi.ChatActionBar.ChatActionBarJoinRequest:
                            Button joinRequest = new();
                            joinRequest.Content = "Join request";
                            ActionBarSetGridPosition(joinRequest);
                            break;
                        case TdApi.ChatActionBar.ChatActionBarReportAddBlock:
                            Button reportAddBlock = new();
                            reportAddBlock.Content = "Report";
                            ActionBarSetGridPosition(reportAddBlock);
                            break;
                        case TdApi.ChatActionBar.ChatActionBarReportSpam:
                            Button reportSpam = new();
                            reportSpam.Content = "Report";
                            ActionBarSetGridPosition(reportSpam);
                            break;
                        case TdApi.ChatActionBar.ChatActionBarReportUnrelatedLocation:
                            Button reportUnrelatedLocation = new();
                            reportUnrelatedLocation.Content = "Report";
                            ActionBarSetGridPosition(reportUnrelatedLocation);
                            break;
                        case TdApi.ChatActionBar.ChatActionBarSharePhoneNumber:
                            Button sharePhoneNumber = new();
                            sharePhoneNumber.Content = "Share phone number";
                            ActionBarSetGridPosition(sharePhoneNumber);
                            break;
                    }

                    void ActionBarSetGridPosition(Button button)
                    {
                        GridChatActions.Children.Add(button);
                        if (_chatActionColumns >= 1)
                        {
                            button.Margin = new Thickness(0, 4, 0, 0);
                            _chatActionColumns = 0;
                            _chatActionRows++;
                            Grid.SetRow(button, _chatActionRows);
                            Grid.SetColumn(button, 0);
                            _chatActionColumns++;
                        }
                        else
                        {
                            button.Margin = new Thickness(4, 0, 4, 0);
                            Grid.SetColumn(button, _chatActionColumns);
                            _chatActionColumns++;
                        }
                    }
                });
            }
            else
            {
                await DispatcherQueue.EnqueueAsync(() => GridChatActions.Visibility = Visibility.Collapsed);
            }

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

                if (_background.Document != null)
                {
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
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
    
            var settings = SettingsService.LoadSettings();
            // IconMedia.Glyph = settings.StartMediaPage switch
            // {
            //     "Emojis" => "&#xE76E;",
            //     "Stickers" => "&#xF4AA;",
            //     "GIFs" => "&#xF4A9;",
            //     _ => string.Empty
            // }; TODO: Fix emojis
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        async Task OpenChatList()
        {
            try
            {
                ContentFrame.Navigate(typeof(ChatHistoryList));
                if (ContentFrame.Content is ChatHistoryList chatHistoryList)
                    await chatHistoryList.PrepareChatList(this, _chat, _messageService, _replyService);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        async Task OpenMessageThread(long messageThreadId)
        {
            try
            {
                if (ContentFrame.Content != null) ContentFrame.Content = null;
                ContentFrame.Navigate(typeof(MessageThreadList));
                if (ContentFrame.Content is MessageThreadList messageThreadList)
                    await messageThreadList.PrepareMessageThread(_chat.Id, messageThreadId, this, _messageService, _replyService);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
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
            GridUserActions.Height = 100;
        });
    }
        
    public void CloseChat()
    {
        // if (MessagesList.Children.Count > 0) MessagesList.Children.Clear();
        _client.CloseChatAsync(_chatId);
        _ChatsView.CloseChat();
    }
        
    private void ContextMenuNotifications_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private async void ContextMenuViewGroupInfo_OnClick(object sender, RoutedEventArgs e)
    {
        await UserService.ShowProfile(chat: _chat, xamlRoot: XamlRoot);
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

    private async void ContextMenuCreatePoll_OnClick(object sender, RoutedEventArgs e)
    {
        await CreatePoll.ShowAsync();
    }

    private async void ContextMenuReport_OnClick(object sender, RoutedEventArgs e)
    {
        await DialogReportChat.ShowAsync();
    }

    private void ContextMenuClearHistory_OnClick(object sender, RoutedEventArgs e)
    {
        if (_chat.CanBeDeletedOnlyForSelf)
        {
            _client.ExecuteAsync(new TdApi.DeleteChatHistory
            {
                ChatId = _chatId, RemoveFromChatList = false, Revoke = true
            });
        }
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
            Debug.WriteLine(e);
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
                CloseChat();
                break;
        }
    }

    private void ButtonAttachMedia_OnClick(object sender, RoutedEventArgs e)
    {
        ContextMenuMedia.ShowAt(ButtonAttachMedia);
    }

    private async void ContextMenuPhotoOrVideo_OnClick(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FileOpenPicker();

        var mainWindow = (Application.Current as App)?._mWindow;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        folderPicker.FileTypeFilter.Add(".png");
        folderPicker.FileTypeFilter.Add(".jpg");
        folderPicker.FileTypeFilter.Add(".jpeg");
        folderPicker.FileTypeFilter.Add(".mp4");
        folderPicker.FileTypeFilter.Add(".mov");
        folderPicker.FileTypeFilter.Add(".m4v");
        var mediaFiles = await folderPicker.PickMultipleFilesAsync();
        
        if (mediaFiles != null) await GenerateMediaFiles(mediaFiles);
    }

    private async void ContextMenuFile_OnClick(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FileOpenPicker();

        var mainWindow = (Application.Current as App)?._mWindow;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        folderPicker.FileTypeFilter.Add("*");
        var documentFiles = await folderPicker.PickMultipleFilesAsync();
        if (documentFiles != null) await GenerateMediaFiles(documentFiles);
    }

    private async Task GenerateMediaFiles(IReadOnlyList<StorageFile> mediaFiles)
    {
        foreach (var mediaFile in mediaFiles)
        {
            if (!_mediaFiles.Contains(mediaFile)) _mediaFiles.Add(mediaFile);
            var extension = Path.GetExtension(mediaFile.Name)?.ToLower();

            switch (extension)
            {
                case ".png" or ".jpg" or ".jpeg" or ".webp":
                {
                    var image = new Image();
                    image.Source = new BitmapImage(new Uri(mediaFile.Path));
                    image.Stretch = Stretch.Fill;
                    image.Margin = new Thickness(0, 0, 0, 4);
                    FlipViewMedia.Items.Add(image);
                    break;
                }
                case ".mp4" or ".mov" or ".m4v" or ".m4a" or ".webm" or ".gif":
                {
                    var video = new MediaPlayerElement();
                    video.Source = MediaSource.CreateFromUri(new Uri(mediaFile.Path));
                    video.AutoPlay = true;
                    video.Stretch = Stretch.Fill;
                    video.MediaPlayer.Volume = 0;
                    video.Margin = new Thickness(0, 0, 0, 4);
                    FlipViewMedia.Items.Add(video);
                    break;
                }
                case ".mp3" or ".ogg" or ".wav":
                    var audioStackPanel = new StackPanel();
                    audioStackPanel.Orientation = Orientation.Horizontal;
                    audioStackPanel.Margin = new Thickness(4);
                    audioStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                    audioStackPanel.VerticalAlignment = VerticalAlignment.Top;
                    
                    var playButton = new Button();
                    playButton.Content = "\u25B6";
                    playButton.Margin = new Thickness(0, 0, 4, 0);
                    audioStackPanel.Children.Add(playButton);
                    
                    var audioNameTextBlock = new TextBlock();
                    audioNameTextBlock.Text = mediaFile.Name;
                    audioNameTextBlock.TextWrapping = TextWrapping.Wrap;
                    audioStackPanel.Children.Add(audioNameTextBlock);
                    FlipViewMedia.Items.Add(audioStackPanel);
                    break;
                default:
                    var stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Vertical;
                    stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    stackPanel.VerticalAlignment = VerticalAlignment.Center;
                    
                    var border = new Border();
                    border.CornerRadius = new CornerRadius(4);
                    border.Margin = new Thickness(0, 0, 4, 0);
                    border.Child = new FontIcon{Glyph = "\uE7C3", FontSize = 20, Margin = new Thickness(4)};
                    border.Padding = new Thickness(0);
                    stackPanel.Children.Add(border);
                    
                    var textBlock = new TextBlock();
                    textBlock.Text = mediaFile.Name;
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.FontSize = 16;
                    stackPanel.Children.Add(textBlock);
                    FlipViewMedia.Items.Add(stackPanel);
                    break;
            }
        }

        var openedPopups = VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot);
        if (openedPopups.Count <= 0 && FlipViewMedia.Items.Count > 0)
        {
            await DialogSendMediaMessage.ShowAsync();
        }
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

    private async void TopBar_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
        {
            await UserService.ShowProfile(chat: _chat, xamlRoot: XamlRoot);
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
        // MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);
    }

    private async void DialogReportChat_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        await _client.ExecuteAsync(new TdApi.ReportChat
        {
            ChatId = _chatId,
            Text = TextBoxReportDetails.Text
        });
    }

    private void ButtonChatEventLog_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void DialogSendMediaMessage_OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _mediaFiles.Clear();
        FlipViewMedia.Items.Clear();
    }

    private async void DialogSendMediaMessage_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_mediaFiles.Count >= 2)
        {
            List<TdApi.InputMessageContent> inputMessageContents = [];
            bool isCaptionAlreadyAdded = false;
            
            foreach (var mediaFile in _mediaFiles)
            {
                var extension = Path.GetExtension(mediaFile.Path)?.ToLower();
                switch (extension)
                {
                    case ".png" or ".jpg" or ".jpeg" or ".webp":
                        inputMessageContents.Add(new TdApi.InputMessageContent.InputMessagePhoto
                        {
                            Photo = new TdApi.InputFile.InputFileLocal
                            {
                                Path = mediaFile.Path
                            },
                            Caption = isCaptionAlreadyAdded ? null 
                                : new TdApi.FormattedText {Text = TextBoxCaption.Text}
                        });
                        if (!isCaptionAlreadyAdded) isCaptionAlreadyAdded = true;
                        break;
                    case ".mp4" or ".mov" or ".m4v" or ".m4a" or ".webm":
                        inputMessageContents.Add(new TdApi.InputMessageContent.InputMessageVideo
                        {
                            Video = new TdApi.InputFile.InputFileLocal
                            {
                                Path = mediaFile.Path
                            },
                            Caption = isCaptionAlreadyAdded ? null 
                                : new TdApi.FormattedText {Text = TextBoxCaption.Text}
                        });
                        if (!isCaptionAlreadyAdded) isCaptionAlreadyAdded = true;
                        break;
                    case ".mp3" or ".ogg" or ".wav":
                        inputMessageContents.Add(new TdApi.InputMessageContent.InputMessageAudio
                        {
                            Audio = new TdApi.InputFile.InputFileLocal
                            {
                                Path = mediaFile.Path
                            },
                            Caption = isCaptionAlreadyAdded ? null 
                                : new TdApi.FormattedText {Text = TextBoxCaption.Text}
                        });
                        if (!isCaptionAlreadyAdded) isCaptionAlreadyAdded = true;
                        break;
                    case ".gif":
                        inputMessageContents.Add(new TdApi.InputMessageContent.InputMessageAnimation
                        {
                            Animation = new TdApi.InputFile.InputFileLocal
                            {
                                Path = mediaFile.Path
                            },
                            Caption = isCaptionAlreadyAdded ? null 
                                : new TdApi.FormattedText {Text = TextBoxCaption.Text}
                        });
                        if (!isCaptionAlreadyAdded) isCaptionAlreadyAdded = true;
                        break;
                    default:
                        inputMessageContents.Add(new TdApi.InputMessageContent.InputMessageDocument
                        {
                            Document = new TdApi.InputFile.InputFileLocal
                            {
                                Path = mediaFile.Path
                            },
                            Caption = isCaptionAlreadyAdded ? null 
                                : new TdApi.FormattedText {Text = TextBoxCaption.Text}
                        });
                        if (!isCaptionAlreadyAdded) isCaptionAlreadyAdded = true;
                        break;
                }
            }

            if (_replyService.GetReplyMessageId() != 0)
            {
                await _client.ExecuteAsync(new TdApi.SendMessageAlbum
                {
                    ChatId = _chatId,
                    MessageThreadId = 0,
                    ReplyTo = new TdApi.InputMessageReplyTo.InputMessageReplyToMessage {
                        MessageId = _replyService.GetReplyMessageId(),},
                    Options = null,
                    InputMessageContents = inputMessageContents.ToArray()
                });
            }
            else
            {
                await _client.ExecuteAsync(new TdApi.SendMessageAlbum
                {
                    ChatId = _chatId,
                    MessageThreadId = 0,
                    ReplyTo = null,
                    Options = null,
                    InputMessageContents = inputMessageContents.ToArray()
                });
            }
        }
    }

    private void DialogSendMediaMessage_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
        ContextMenuPhotoOrVideo_OnClick(sender, null);
    }
}