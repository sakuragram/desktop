using System;
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

    private TdApi.Message _message;
    private TdApi.MessageContent _messageContent;
    
    private TextBlock _textMessage;
    private TextBlock _caption;
    private Image _photoMessage;
    private Image _stickerStaticMessage;
    private MediaPlayerElement _stickerDynamicMessage;
    private Image _videoMessage;
    private Image _voiceNoteMessage;
    private Image _documentMessage;
    private Image _audioMessage;
    private Image _videoNoteMessage;
    private Image _pollMessage;
    private Image _paidMediaMessage;
    private Image _contactMessage;

    /** can be used only for photo, video, audio and document messages */
    public long _mediaAlbumId;
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
            case TdApi.Update.UpdateMessageInteractionInfo updateMessageInteractionInfo:
            {
                break;
            }
            // case TdApi.Update.UpdateMessageEdited:
            // {
            //     var message = _client.ExecuteAsync(new TdApi.GetMessage
            //     {
            //         ChatId = _chatId,
            //         MessageId = _messageId
            //     });
            //
            //     textMessage.Text = message.Result.Content switch
            //     {
            //         TdApi.MessageContent.MessageText messageText => textMessage.Text = messageText.Text.Text,
            //         _ => textMessage.Text
            //     };
            //     break;
            // }
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

        if (sender.User != null)
        {
            await ProfilePicture.InitializeProfilePhoto(sender.User, null, canOpenProfile: true);
            var emojiStatus = await _client.GetDefaultChatEmojiStatusesAsync();
            if (sender.User.Id == currentUser.Id) 
                MessageBackground.Background = (Brush)Application.Current.Resources["SolidBackgroundFillColorBaseAltBrush"];
        }
        else if (sender.Chat != null)
        {
            await ProfilePicture.InitializeProfilePhoto(null, sender.Chat, canOpenProfile: true);
        }

        if (sender.User != null)
        {
            if (sender.User.Id == currentUser.Id) DisplayName.Visibility = Visibility.Collapsed;
            else SetDisplayName();
        }
        else SetDisplayName();
        
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

            Reply.Visibility = Visibility.Visible;
        }

        #endregion
        
        #region ForwardInfo

        if (message.ForwardInfo != null)
        {
            if (message.ForwardInfo.Source != null)
            {
                TextBlockForwardInfo.Text = $"Forwarded from {message.ForwardInfo.Source.SenderName}";
                TextBlockForwardInfo.Visibility = Visibility.Visible;
            }
            else if (message.ForwardInfo.Origin != null)
            {
                switch (message.ForwardInfo.Origin)
                {
                    case TdApi.MessageOrigin.MessageOriginChannel channel:
                    {
                        var forwardInfo = string.Empty;

                        forwardInfo = chat.Title;

                        if (channel.AuthorSignature != string.Empty)
                            forwardInfo = forwardInfo + $" ({channel.AuthorSignature})";

                        TextBlockForwardInfo.Text = $"Forwarded from {forwardInfo}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginChat originChat:
                    {
                        TextBlockForwardInfo.Text = $"Forwarded from {originChat.AuthorSignature}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginUser user:
                    {
                        var originUser = _client.GetUserAsync(user.SenderUserId).Result;
                        TextBlockForwardInfo.Text = $"Forwarded from {originUser.FirstName} {originUser.LastName}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginHiddenUser hiddenUser:
                    {
                        TextBlockForwardInfo.Text = $"Forwarded from {hiddenUser.SenderName}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                }
            }
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
                GenerateTextMessage(messageText);
                break;
            case TdApi.MessageContent.MessageSticker messageSticker:
                CheckSticker(messageSticker);
                GenerateStickerMessage(messageSticker);
                break;
            case TdApi.MessageContent.MessageAnimation messageAnimation:
                GenerateAnimationMessage(messageAnimation);
                break;
            case TdApi.MessageContent.MessagePhoto messagePhoto:
                GeneratePhotoMessage(messagePhoto);
                break;
            case TdApi.MessageContent.MessageDocument messageDocument:
                DocumentType documentType = new(messageDocument.Document);
                if (_mediaElementsCount > 0) PanelMessageContent.Children.Insert(_mediaElementsCount + 1, documentType);
                else PanelMessageContent.Children.Add(documentType);
                _mediaElementsCount++;
                
                if (!string.IsNullOrEmpty(messageDocument.Caption.Text))
                {
                    TextBlock captionTextBlock = new();
                    captionTextBlock.Text = messageDocument.Caption.Text;
                    captionTextBlock.IsTextSelectionEnabled = true;
                    captionTextBlock.TextWrapping = TextWrapping.Wrap;
                    PanelMessageContent.Children.Add(captionTextBlock);
                }
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

    private async void GenerateAnimationMessage(TdApi.MessageContent.MessageAnimation messageAnimation)
    {
        MediaPlayerElement mediaPlayer = new();
        if (messageAnimation.Animation.Animation_.Local.IsDownloadingCompleted) 
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(messageAnimation.Animation.Animation_.Local.Path));
        else await _client.DownloadFileAsync(messageAnimation.Animation.Animation_.Id, 1, synchronous:true)
                .ContinueWith(_ =>
                {
                    mediaPlayer.Source = 
                        MediaSource.CreateFromUri(new Uri(messageAnimation.Animation.Animation_.Local.Path));
                });
        PanelMessageContent.Children.Add(mediaPlayer);
        mediaPlayer.MediaPlayer.AutoPlay = true;
    }

    public void AddAlbumElement(TdApi.Message message)
    {
        if (message.MediaAlbumId == 0) return;
        if (message.MediaAlbumId == _mediaAlbumId)
        {
            GenerateMessageFromContent(message.Content);
        }
    }
    
    private async void GeneratePhotoMessage(TdApi.MessageContent.MessagePhoto messagePhoto)
    {
        // _photoMessage = new();
        // _photoMessage.Width = messagePhoto.Photo.Sizes[1].Width * (1.0 / 1.5);
        // _photoMessage.Height = messagePhoto.Photo.Sizes[1].Height * (1.0 / 1.5);
        //
        // if (messagePhoto.Photo.Sizes[1].Photo.Local.Path != string.Empty)
        // {
        //     _photoMessage.Source = new BitmapImage(new Uri(messagePhoto.Photo.Sizes[1].Photo.Local.Path));
        // }
        // else
        // {
        //     await _client.DownloadFileAsync(messagePhoto.Photo.Sizes[1].Photo.Id, 1).ContinueWith(_ => {
        //             _photoMessage.Source = new BitmapImage(new Uri(messagePhoto.Photo.Sizes[1].Photo.Local.Path));
        //         });
        // }
        
        if (messagePhoto.Caption != null)
        {
            _caption = new();
            _caption.Text = messagePhoto.Caption.Text;
            _caption.IsTextSelectionEnabled = true;
            _caption.TextWrapping = TextWrapping.Wrap;
        }

        if (messagePhoto.ShowCaptionAboveMedia)
        {
            PanelMessageContent.Children.Add(_caption);
            //PanelMessageContent.Children.Add(_photoMessage);
        }
        else
        {
            //PanelMessageContent.Children.Add(_photoMessage);
            PanelMessageContent.Children.Add(_caption);
        }
        
        // if (album != null && mediaAlbumId != 0)
        // {
        //     _mediaAlbum = album;
        //     _mediaAlbumId = mediaAlbumId;
        //
        //     foreach (var media in _mediaAlbum)
        //     {
        //         if (media.MediaAlbumId == _mediaAlbumId)
        //         {
        //             switch (media.Content)
        //             {
        //                 case TdApi.MessageContent.MessagePhoto mediaPhoto:
        //                 {
        //                     Image image = new();
        //                     image.Width = mediaPhoto.Photo.Sizes[1].Width * (1.0 / 1.5);
        //                     image.Height = mediaPhoto.Photo.Sizes[1].Height * (1.0 / 1.5);
        //
        //                     if (mediaPhoto.Photo.Sizes[1].Photo.Local.Path != string.Empty)
        //                     {
        //                         image.Source = new BitmapImage(new Uri(mediaPhoto.Photo.Sizes[1].Photo.Local.Path));
        //                     }
        //                     else
        //                     {
        //                         await _client.DownloadFileAsync(mediaPhoto.Photo.Sizes[1].Photo.Id, 1).ContinueWith(_ => {
        //                             image.Source = new BitmapImage(new Uri(mediaPhoto.Photo.Sizes[1].Photo.Local.Path));
        //                         });
        //                     }
        //                     break;
        //                 }
        //                 case TdApi.MessageContent.MessageVideo mediaVideo:
        //                 {
        //                     break;
        //                 }
        //             }
        //         }
        //     }
        // }
    }

    private void GenerateTextMessage(TdApi.MessageContent.MessageText messageText)
    {
        TextBlock textMessage = new();
        textMessage.Text = messageText.Text.Text;
        textMessage.IsTextSelectionEnabled = true;
        textMessage.TextWrapping = TextWrapping.Wrap;
        
        var tempSpan = new Span();
        var text = messageText.Text.Text;

        int lastIndex = 0;

        foreach (var entity in messageText.Text.Entities)
        {
            switch (entity.Type)
            {
                case TdApi.TextEntityType.TextEntityTypeUrl:
                {
                    var regex = new Regex(@"(https?://[^\s]+)");
                    var match = regex.Match(text, lastIndex);

                    if (match.Success)
                    {
                        var link = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri(link),
                            Foreground = new SolidColorBrush(Colors.Azure),
                            TextDecorations = TextDecorations.Underline
                        };
                        hyperlink.Inlines.Add(new Run { Text = link });

                        tempSpan.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        tempSpan.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeTextUrl url:
                {
                    var hyperlink = new Hyperlink
                    {
                        NavigateUri = new Uri(url.Url),
                        Foreground = new SolidColorBrush(Colors.Azure),
                        TextDecorations = TextDecorations.Underline
                    };
                    hyperlink.Inlines.Add(new Run { Text = text });
                    
                    tempSpan.Inlines.Add(new Run { Text = text.Substring(lastIndex, entity.Length - lastIndex) });
                    tempSpan.Inlines.Add(hyperlink);
                    lastIndex = entity.Offset + entity.Length;
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeHashtag:
                {
                    var regex = new Regex(@"(#\w+)");
                    var match = regex.Match(text);
                    
                    if (match.Success)
                    {
                        var hashtag = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri("https://t.me/sakuragram/"),
                            Foreground = new SolidColorBrush(Colors.Azure)
                        };
                        hyperlink.Inlines.Add(new Run { Text = hashtag });
                        
                        tempSpan.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        tempSpan.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeMentionName mentionName:
                {
                    var regex = new Regex(@"(@\w+)");
                    var match = regex.Match(text);
                    
                    if (match.Success)
                    {
                        var mention = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri($"https://t.me/{mentionName.UserId}"),
                            Foreground = new SolidColorBrush(Colors.Azure)
                        };
                        hyperlink.Inlines.Add(new Run { Text = mention });
                        
                        tempSpan.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        tempSpan.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeMention:
                {
                    var regex = new Regex(@"(@\w+)");
                    var match = regex.Match(text);
                    
                    if (match.Success)
                    {
                        var mention = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri($"https://t.me/{mention.Replace("@", string.Empty)}"),
                            Foreground = new SolidColorBrush(Colors.Azure)
                        };
                        hyperlink.Inlines.Add(new Run { Text = mention });
                        
                        tempSpan.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        tempSpan.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeBold:{
                    string boldText = text.Substring(entity.Offset, entity.Length);
                    
                    textMessage.Inlines.Add(new Run { Text = text.Substring(0, entity.Offset) });
                    textMessage.Inlines.Add(new Run { Text = boldText, FontWeight = FontWeights.Bold, FontStyle = FontStyle.Italic });
                    textMessage.Inlines.Add(new Run { Text = text.Substring(entity.Offset + entity.Length) });
                    break;
                }
            }
        }

        tempSpan.Inlines.Add(new Run { Text = text.Substring(lastIndex) });

        textMessage.Inlines.Clear();
        textMessage.Inlines.Add(tempSpan);
        PanelMessageContent.Children.Add(textMessage);
    }

    private async void CheckSticker(TdApi.MessageContent.MessageSticker messageSticker)
    {
        if (messageSticker.Sticker.Sticker_.Local.IsDownloadingCompleted)
        {
            return;
        }
        else
        {
            await _client.DownloadFileAsync(messageSticker.Sticker.Sticker_.Id, 3);
            return;
        }
    }
    
    private async void GenerateStickerMessage(TdApi.MessageContent.MessageSticker messageSticker)
    {
        switch (messageSticker.Sticker.Format)
        {
            case TdApi.StickerFormat.StickerFormatWebp:
            {
                try
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        _stickerStaticMessage = new();
                        _stickerStaticMessage.Source = new BitmapImage(new Uri(messageSticker.Sticker.Sticker_.Local.Path));
                        _stickerStaticMessage.Width = messageSticker.Sticker.Width * (1.0 / 3);
                        _stickerStaticMessage.Height = messageSticker.Sticker.Height * (1.0 / 3);
                        PanelMessageContent.Children.Add(_stickerStaticMessage);
                    });
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }
                break;
            }
            case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
            {
                try
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        _stickerDynamicMessage = new();
                        _stickerDynamicMessage.Source = MediaSource.CreateFromUri(new Uri(messageSticker.Sticker.Sticker_.Local.Path));
                        _stickerDynamicMessage.Width = messageSticker.Sticker.Width * (1.0 / 3);
                        _stickerDynamicMessage.Height = messageSticker.Sticker.Height * (1.0 / 3);
                        _stickerDynamicMessage.MediaPlayer.IsLoopingEnabled = true;
                        _stickerDynamicMessage.MediaPlayer.AutoPlay = true;
                        _stickerDynamicMessage.MediaPlayer.Volume = 0;
                        _stickerDynamicMessage.MediaPlayer.Position = TimeSpan.Zero;
                        _stickerDynamicMessage.MediaPlayer.Play();
                        PanelMessageContent.Children.Add(_stickerDynamicMessage);
                    });
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }
                break;
            }
        }
    }
    
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
                MessageIds = new long[] { _messageId },
                Revoke = Revoke.IsChecked.Value
            });
    }

    private void ChatMessage_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        // if (_messageService._isMessageSelected) Select_OnClick(null, null);
    }

    private async void ButtonReply_OnClick(object sender, RoutedEventArgs e)
    {
        var replies = await _chat.GetMessagesAsync(_chatId, true, _messageId, 0);
        await _chat.GenerateMessageByType(replies, true);
    }

    private async void DisplayName_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var messageSender = await UserService.GetSender(_message.SenderId);
        await UserService.ShowProfile(messageSender.User, messageSender.Chat);
    }
}