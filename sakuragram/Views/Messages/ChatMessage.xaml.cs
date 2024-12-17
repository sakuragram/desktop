﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
using Windows.UI.Input;
using Windows.UI.Text;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Controls.Core;
using sakuragram.Controls.Messages;
using sakuragram.Services;
using sakuragram.Services.Chat;
using sakuragram.Services.Core;
using sakuragram.Views.Messages.MessageElements;
using TdLib;

namespace sakuragram.Views.Chats.Messages;

public partial class ChatMessage : Page
{
    private static TdClient _client = App._client;
    private bool _isContextMenuOpen = false;

    private long _chatId;
    public long _messageId;
    private TdApi.ProfilePhoto _profilePhoto;

    #region MessageContent

    public TdApi.Message _message;
    private TdApi.MessageContent _messageContent;

    /** can be used only for photo, video, audio and document messages */
    private MediaAlbum _mediaFlipView;
    public long _mediaAlbumId;
    public bool _canAttachAlbumElements = false;
    private List<TdApi.Message> _mediaAlbum;
    private int _mediaElementsCount = 0;

    #endregion

    public ReplyService _replyService;
    public MessageService _messageService;

    public Chat _chat;
    
    private bool _bIsSelected = false;

    private int _reactionGridRows = 0;
    private int _reactionGridColumns = 0;
    
    public ChatMessage(Chat chat, ReplyService replyService)
    {
        InitializeComponent();
        _chat = chat;
        _replyService = replyService;
    }

    private Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateNewMessage updateNewMessage:
            {
                if (updateNewMessage.Message.ChatId == _chatId &&
                    updateNewMessage.Message.MediaAlbumId == _mediaAlbumId)
                {
                    switch (updateNewMessage.Message.Content)
                    {
                        case TdApi.MessageContent.MessagePhoto messagePhoto:
                        {
                            GeneratePhotoMessage(messagePhoto);
                            break;
                        }
                    }
                }
                break;
            }
        }

        return Task.CompletedTask;
    }

    public async Task UpdateMessage(TdApi.Message message)
    {
        _message = message;
        _chatId = message.ChatId;
        _messageId = message.Id;
        _mediaAlbumId = message.MediaAlbumId;
        var chat = await _client.GetChatAsync(_chatId);
        var currentUser = await _client.GetMeAsync();

        var sender = await UserService.GetSender(message.SenderId);

        if (_mediaAlbumId != 0) _canAttachAlbumElements = true;
        
        if (sender.User != null)
        {
            await ProfilePicture.InitializeProfilePhoto(sender.User, canOpenProfile: true);
            if (sender.User.Id == currentUser.Id)
            {
                GridUserInfo.Visibility = Visibility.Collapsed;
                MessageBackground.Background =
                    (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorBaseBrush"];
                BorderReply.Background = (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorDefaultBrush"];
                ReplyFirstName.Foreground = (Brush)Application.Current.Resources["TextOnAccentFillColorPrimaryBrush"];
                ReplyInputContent.Foreground = (Brush)Application.Current.Resources["TextOnAccentFillColorSecondaryBrush"];
            }
            else SetDisplayName();
        }
        else if (sender.Chat != null)
        {
            await ProfilePicture.InitializeProfilePhoto(null, sender.Chat, canOpenProfile: true);
            SetDisplayName();
        }
        
        TextBlockSendTime.Text = string.IsNullOrEmpty(message.AuthorSignature) 
            ? MathService.CalculateDateTime(message.Date).ToShortTimeString()
            : message.AuthorSignature + ", " + MathService.CalculateDateTime(message.Date).ToShortTimeString();
        
        IconEdited.Visibility = message.EditDate != 0 ? Visibility.Visible : Visibility.Collapsed;

        #region ReplyInfo

        if (message.ReplyTo != null)
        {
            var replyMessage = await _client.ExecuteAsync(new TdApi.GetRepliedMessage
            {
                ChatId = message.ChatId,
                MessageId = _messageId
            });

            var replySender = await UserService.GetSender(replyMessage.SenderId);

            if (replySender.User != null) // if senderId > 0 then it's a user
            {
                ReplyFirstName.Text = $"{replySender.User.FirstName} {replySender.User.LastName}";
            }
            else if (replySender.Chat != null) // if senderId < 0 then it's a chat
            {
                ReplyFirstName.Text = replySender.Chat.Title;
            }

            ReplyInputContent.Text = await MessageService.GetTextMessageContent(replyMessage);

            BorderReply.Visibility = Visibility.Visible;
        }

        #endregion
        
        #region ForwardInfo

        if (message.ForwardInfo != null)
        {
            var forwardInfo = await UserService.GetSenderName(message, true);
            TextBlockForwardInfo.Text = "forwarded from " + forwardInfo;
            TextBlockForwardInfo.Visibility = Visibility.Visible;
        }
        else
        {
            TextBlockForwardInfo.Text = string.Empty;
            TextBlockForwardInfo.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region InretactionInfo

        if (message.InteractionInfo != null)
        {
            if (message.InteractionInfo.Reactions != null)
            {
                foreach (var reaction in message.InteractionInfo.Reactions.Reactions)
                {
                    GenerateReactions(reaction);
                }
            }
        
            if (message.InteractionInfo.ViewCount > 0 && message.IsChannelPost)
            {
                TextBlockViews.Text = message.InteractionInfo.ViewCount.ToString();
                PanelViews.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockViews.Text = string.Empty;
                TextBlockViews.Visibility = Visibility.Collapsed;
            }

            if (message.InteractionInfo.ReplyInfo != null)
            {
                if (chat.Type is TdApi.ChatType.ChatTypeSupergroup { IsChannel: true })
                {
                    PanelReply.Visibility = Visibility.Visible;
                    TextBlockLastReply.Text = "Leave a comment";
                }
                else if (chat.Type is TdApi.ChatType.ChatTypeSupergroup { IsChannel: false })
                {
                    PanelReply.Visibility = Visibility.Collapsed;
                }
                
                if (message.InteractionInfo.ReplyInfo.ReplyCount > 0)
                {
                    TextBlockReplies.Text = message.InteractionInfo.ReplyInfo.ReplyCount.ToString();
                    PanelReplies.Visibility = Visibility.Visible;

                    if (chat.Type is TdApi.ChatType.ChatTypeSupergroup { IsChannel: true })
                    {
                        PanelReplies.Visibility = Visibility.Collapsed;
                        PanelReply.Visibility = Visibility.Visible;
                        TextBlockLastReply.Text = message.InteractionInfo.ReplyInfo.ReplyCount + " comments";

                        // try
                        // {
                        //     var firstLastReplySender =
                        //         await UserService.GetSender(message.InteractionInfo.ReplyInfo.RecentReplierIds[0]);
                        //     var secondLastReplySender =
                        //         await UserService.GetSender(message.InteractionInfo.ReplyInfo.RecentReplierIds[1]);
                        //     var thirdLastReplySender =
                        //         await UserService.GetSender(message.InteractionInfo.ReplyInfo.RecentReplierIds[2]);
                        //
                        //     await PhotoFirstLastReply.InitializeProfilePhoto(firstLastReplySender.User,
                        //         firstLastReplySender.Chat, 16);
                        //     await PhotoSecondLastReply.InitializeProfilePhoto(secondLastReplySender.User,
                        //         secondLastReplySender.Chat, 16);
                        //     await PhotoThirdLastReply.InitializeProfilePhoto(thirdLastReplySender.User,
                        //         thirdLastReplySender.Chat, 16);
                        // }
                        // catch (Exception e)
                        // {
                        //     Console.WriteLine(e);
                        //     throw;
                        // }
                    }
                }
                else
                {
                    TextBlockReplies.Text = string.Empty;
                    PanelReplies.Visibility = Visibility.Collapsed;
                }
            }
            else PanelReplies.Visibility = Visibility.Collapsed;
        }
        else
        {
            PanelViews.Visibility = Visibility.Collapsed;
            PanelReplies.Visibility = Visibility.Collapsed;
            GridReactions.Visibility = Visibility.Collapsed;
        }
        
        #endregion

        #region MessageContent

        GenerateMessageFromContent(message.Content);
        
        #endregion

        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };

        void SetDisplayName()
        {
            DisplayName.Visibility = Visibility.Visible;
            DisplayName.Text = DisplayName.Text = sender.User == null 
                ? sender.Chat?.Title 
                : (sender.User.LastName ?? string.Empty) == string.Empty 
                    ? sender.User.FirstName 
                    : sender.User.FirstName + " " + sender.User.LastName;
        }
    }

    private void GenerateMessageFromContent(TdApi.MessageContent messageContent)
    {
        switch (messageContent)
        {
            case TdApi.MessageContent.MessageText messageText:
                GenerateTextMessage(messageText.Text);
                break;
            case TdApi.MessageContent.MessageSticker messageSticker:
                GenerateStickerMessage(messageSticker);
                break;
            case TdApi.MessageContent.MessageAnimation messageAnimation:
                GenerateAnimationMessage(messageAnimation);
                break;
            case TdApi.MessageContent.MessagePhoto messagePhoto:
                GeneratePhotoMessage(messagePhoto);
                break;
            case TdApi.MessageContent.MessageVideo messageVideo:
                GenerateVideoMessage(messageVideo);
                break;
            case TdApi.MessageContent.MessageVideoNote messageVideoNote:
                GenerateVideoNoteMessage(messageVideoNote);
                break;
            case TdApi.MessageContent.MessageDocument messageDocument:
                GenerateDocumentMessage(messageDocument);
                break;
            case TdApi.MessageContent.MessageUnsupported:
                TextBlock textUnsupported = new();
                textUnsupported.Text = "Unsupported message type";
                textUnsupported.IsTextSelectionEnabled = true;
                PanelMessageContent.Children.Add(textUnsupported);
                break;
            default:
                TextBlock textNotFound = new();
                textNotFound.Text = "Unknown message type";
                textNotFound.IsTextSelectionEnabled = true;
                PanelMessageContent.Children.Add(textNotFound);
                break;
        }
    }

    public void AddAlbumElement(TdApi.Message message)
    {
        if (message.MediaAlbumId == 0) return;
        if (message.MediaAlbumId == _mediaAlbumId)
        {
            GenerateMessageFromContent(message.Content);
        }
    }
    
    #region GeneratingContent
    
    private void GenerateDocumentMessage(TdApi.MessageContent.MessageDocument messageDocument)
    {
        DispatcherQueue.EnqueueAsync(async () =>
        {
            var document = new DocumentType(messageDocument.Document);
            
            if (messageDocument.Caption != null)
            {
                var inlines = await MessageService.GetTextEntities(messageDocument.Caption);
                TextBlockCaption.Inlines.Add(inlines);
            }

            if (_mediaFlipView == null)
            {
                _mediaFlipView = new MediaAlbum(false) { MaxHeight = 55 };
                PanelMessageContent.Children.Insert(0, _mediaFlipView);
            }
            
            _mediaFlipView.AddAlbumElement(document);
        });
    }
    
    private void GenerateVideoNoteMessage(TdApi.MessageContent.MessageVideoNote messageVideoNote)
    {
        var videoNote = new VideoNoteType(messageVideoNote.VideoNote);
        PanelMessageContent.Children.Add(videoNote);
        MessageBackground.Background = new SolidColorBrush(Colors.Transparent);
        GridUserInfo.Visibility = Visibility.Collapsed;
    }

    private void GenerateVideoMessage(TdApi.MessageContent.MessageVideo messageVideo)
    {
        DispatcherQueue.EnqueueAsync(async () =>
        {
            var video = new VideoType(messageVideo.Video);
            
            if (messageVideo.Caption != null)
            {
                var inlines = await MessageService.GetTextEntities(messageVideo.Caption);
                TextBlockCaption.Inlines.Add(inlines);
            }

            if (_mediaFlipView == null)
            {
                _mediaFlipView = new MediaAlbum { MaxHeight = messageVideo.Video.Height / 2.0 };
                PanelMessageContent.Children.Insert(messageVideo.ShowCaptionAboveMedia ? 1 : 0, _mediaFlipView);
            }
            
            _mediaFlipView.AddAlbumElement(video);
        });
    }

    private async void GenerateAnimationMessage(TdApi.MessageContent.MessageAnimation messageAnimation)
    {
        await DispatcherQueue.EnqueueAsync(async () =>
        {
            var animation = new AnimationType(messageAnimation.Animation);

            if (!string.IsNullOrEmpty(messageAnimation.Caption.Text))
            {
                var inlines = await MessageService.GetTextEntities(messageAnimation.Caption);
                TextBlockCaption.Inlines.Add(inlines);
                PanelMessageContent.Children.Insert(messageAnimation.ShowCaptionAboveMedia ? 1 : 0, animation);
            }
            else
            {
                PanelMessageContent.Children.Insert(0, animation);
                MessageBackground.Background = new SolidColorBrush(Colors.Transparent);
                GridUserInfo.Visibility = Visibility.Collapsed;
                TextBlockCaption.Visibility = Visibility.Collapsed;
            }
        });
    }
    
    private void GeneratePhotoMessage(TdApi.MessageContent.MessagePhoto messagePhoto)
    {
        DispatcherQueue.EnqueueAsync(async () =>
        {
            var photo = new PhotoType(messagePhoto.Photo);
            
            if (messagePhoto.Caption != null)
            {
                var inlines = await MessageService.GetTextEntities(messagePhoto.Caption);
                TextBlockCaption.Inlines.Add(inlines);
            }
            
            if (_mediaFlipView == null)
            {
                _mediaFlipView = new MediaAlbum { MaxHeight = messagePhoto.Photo.Sizes[0].Height / 1.0 };
                PanelMessageContent.Children.Insert(messagePhoto.ShowCaptionAboveMedia ? 1 : 0, _mediaFlipView);
            }
            
            _mediaFlipView.AddAlbumElement(photo);
        });
    }

    private void GenerateTextMessage(TdApi.FormattedText messageText)
    {
        var inlines = MessageService.GetTextEntities(messageText).Result;
        TextBlockCaption.Inlines.Add(inlines);
    }
    
    private void GenerateStickerMessage(TdApi.MessageContent.MessageSticker messageSticker)
    {
        var sticker = new StickerType(messageSticker.Sticker);
        PanelMessageContent.Children.Add(sticker);
        MessageBackground.Background = new SolidColorBrush(Colors.Transparent);
        GridUserInfo.Visibility = Visibility.Collapsed;
    }
    
    #endregion
    
    private async void GenerateReactions(TdApi.MessageReaction reaction)
    {
        await DispatcherQueue.EnqueueAsync(() =>
        {
            Reaction reactionControl = new(reaction, _chatId, _messageId);
            reactionControl.Margin = new Thickness(0, 2, 2, 0);

            if (_reactionGridColumns == 5)
            {
                _reactionGridColumns = 0;
                _reactionGridRows++;
                if (_reactionGridRows >= GridReactions.RowDefinitions.Count)
                {
                    GridReactions.RowDefinitions.Add(new RowDefinition { });
                }
            }

            while (_reactionGridRows >= GridReactions.RowDefinitions.Count)
            {
                GridReactions.RowDefinitions.Add(new RowDefinition { });
            }

            if (_reactionGridColumns >= GridReactions.ColumnDefinitions.Count)
            {
                GridReactions.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(57, GridUnitType.Auto) });
            }

            GridReactions.Children.Add(reactionControl);
            Grid.SetRow(reactionControl, _reactionGridRows);
            Grid.SetColumn(reactionControl, _reactionGridColumns);
        });
        _reactionGridColumns++;
    }

    private void Reply_OnClick(object sender, RoutedEventArgs e)
    {
        _replyService.SelectMessageForReply(_messageId);
    }
 
    private async void Forward_OnClick(object sender, RoutedEventArgs e)
    {
        if (_messageService.GetSelectedMessages().Length == 0) _messageService.SelectMessage(_messageId);
        await ForwardMessageList.ShowAsync();
    }

    private void Edit_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ExecuteAsync(new TdApi.EditMessageText
        {
        });
    }

    private void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        DeleteMessagesConfirmation.ShowAsync();
    }

    private void Pin_OnClick(object sender, RoutedEventArgs e)
    {
        PinMessageConfirmation.ShowAsync();
    }

    private void MessageLink_OnClick(object sender, RoutedEventArgs e)
    {
        var messageLink = _client.GetMessageLinkAsync(_chatId, _messageId);
        var dataPackage = new DataPackage();

        dataPackage.SetText(messageLink.Result.Link);
        Clipboard.SetContent(dataPackage);
    }

    private void Select_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_bIsSelected)
        {
            _messageService.SelectMessage(_messageId);
            MessageBackground.Background = new SolidColorBrush(Colors.Gray);
            _bIsSelected = true;
        }
        else
        {
            _messageService.DeselectMessage(_messageId);
            MessageBackground.Background = new SolidColorBrush(Colors.Black);
            _bIsSelected = false;
        }
    }

    private void Report_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private async void ChatMessage_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        _replyService.SelectMessageForReply(_messageId);
        var message = await _client.GetMessageAsync(_chatId, _messageId);
        await _chat.UpdateReplyInfo(message);
    }

    private async void ForwardMessageList_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        var chats = _client.ExecuteAsync(new TdApi.GetChats
            { ChatList = new TdApi.ChatList.ChatListMain(), Limit = 100 }).Result;

        foreach (var chatId in chats.ChatIds)
        {
            var chat = await _client.ExecuteAsync(new TdApi.GetChat
            {
                ChatId = chatId
            });

            if (!chat.Permissions.CanSendBasicMessages) continue;

            var chatEntry = new ChatEntryForForward
            {
                _fromChatId = _chatId,
                _messageIds = _messageService.GetSelectedMessages()
            };
            ChatList.Children.Add(chatEntry);
            chatEntry.UpdateEntry(chat);
        }
    }

    private void PinMessageConfirmation_OnPrimaryButtonClick(ContentDialog sender,
        ContentDialogButtonClickEventArgs args)
    {
        _client.PinChatMessageAsync(_chatId, _messageId, NotifyAllMembers.IsChecked.Value);
    }

    private void DeleteMessagesConfirmation_OnPrimaryButtonClick(ContentDialog sender,
        ContentDialogButtonClickEventArgs args)
    {
        if (_messageService._isMessageSelected)
            _client.ExecuteAsync(new TdApi.DeleteMessages
            {
                ChatId = _chatId,
                MessageIds = _messageService.GetSelectedMessages(),
                Revoke = Revoke.IsChecked.Value
            });
        else
            _client.ExecuteAsync(new TdApi.DeleteMessages
            {
                ChatId = _chatId,
                MessageIds = [_messageId],
                Revoke = Revoke.IsChecked.Value
            });
    }

    private void ChatMessage_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        // if (_messageService._isMessageSelected) Select_OnClick(null, null);
    }

    private async void ButtonReply_OnClick(object sender, RoutedEventArgs e)
    {
        // var replies = await _chat.GetMessagesAsync(_chatId, true, _messageId, 0);
        // await _chat.GenerateMessageByType(replies, true);
    }

    private async void DisplayName_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var messageSender = await UserService.GetSender(_message.SenderId);
        await UserService.ShowProfile(messageSender.User, messageSender.Chat);
    }
}