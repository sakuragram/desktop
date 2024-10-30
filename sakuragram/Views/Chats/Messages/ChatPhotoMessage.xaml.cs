using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Views.Chats.Messages;

public partial class ChatPhotoMessage : Page
{
    private static TdClient _client = App._client;
    private TdApi.MessageContent _messageMediaContent;
    private long _chatId;
    private int _mediaFileId;
    private List<Image> Photos = [];
    private MediaService _mediaService = new();
    
    public ChatPhotoMessage()
    {
        InitializeComponent();
        
        _client.UpdateReceived += async (_, update) => { await ProcessUpdate(update); };
    }

    private async Task ProcessUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id == _mediaFileId)
                {
                    switch (_messageMediaContent)
                    {
                        case TdApi.MessageContent.MessagePhoto messagePhoto:
                        {
                            if (messagePhoto.Photo.Sizes[1].Photo.Local.Path != string.Empty)
                            {
                                Image.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                                    () => {
                                        Image.ImageSource =
                                            new BitmapImage(new Uri(messagePhoto.Photo.Sizes[1].Photo.Local.Path));
                                        SetImageTransform(messagePhoto);
                                    });
                            }
                            else if (updateFile.File.Local.Path != string.Empty)
                            {
                                Image.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                                    () => {
                                        Image.ImageSource = new BitmapImage(new Uri(updateFile.File.Local.Path));
                                        SetImageTransform(messagePhoto);
                                    });
                            }
                            break;
                        }
                    }
                }
                break;
            }
        }
    }

    public void UpdateMessage(TdApi.Message message)
    {
        _messageMediaContent = message.Content;
        _chatId = message.ChatId;

        var sender = message.SenderId switch
        {
            TdApi.MessageSender.MessageSenderUser u => u.UserId,
            TdApi.MessageSender.MessageSenderChat c => c.ChatId,
            _ => 0
        };

        if (sender > 0) // if senderId > 0 then it's a user
        {
            var user = _client.GetUserAsync(userId: sender).Result;
            DisplayName.Text = user.FirstName + " " + user.LastName;
            MediaService.GetUserPhoto(user, ProfilePicture);
        }
        else // if senderId < 0 then it's a chat
        {
            var chat = _client.GetChatAsync(chatId: sender).Result;
            DisplayName.Text = chat.Title;
            MediaService.GetChatPhoto(chat, ProfilePicture);
        }

        if (message.ReplyTo != null)
        {
            var replyMessage = _client.ExecuteAsync(new TdApi.GetRepliedMessage
            {
                ChatId = message.ChatId,
                MessageId = message.Id
            }).Result;

            var replyUserId = replyMessage.SenderId switch
            {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };

            if (replyUserId > 0) // if senderId > 0 then it's a user
            {
                var replyUser = _client.GetUserAsync(replyUserId).Result;
                ReplyFirstName.Text = $"{replyUser.FirstName} {replyUser.LastName}";
            }
            else // if senderId < 0 then it's a chat
            {
                var replyChat = _client.GetChatAsync(replyUserId).Result;
                ReplyFirstName.Text = replyChat.Title;
            }

            ReplyInputContent.Text = replyMessage.Content switch
            {
                TdApi.MessageContent.MessageText messageText => messageText.Text.Text,
                TdApi.MessageContent.MessageAnimation messageAnimation => messageAnimation.Caption.Text,
                TdApi.MessageContent.MessageAudio messageAudio => messageAudio.Caption.Text,
                TdApi.MessageContent.MessageDocument messageDocument => messageDocument.Caption.Text,
                TdApi.MessageContent.MessagePhoto messagePhoto => messagePhoto.Caption.Text,
                TdApi.MessageContent.MessagePoll messagePoll => messagePoll.Poll.Question.Text,
                TdApi.MessageContent.MessageVideo messageVideo => messageVideo.Caption.Text,
                TdApi.MessageContent.MessagePinMessage => "pinned message",
                TdApi.MessageContent.MessageVoiceNote messageVoiceNote => messageVoiceNote.Caption.Text,
                _ => "Unsupported message type"
            };

            Reply.Visibility = Visibility.Visible;
        }

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
                        string forwardInfo = string.Empty;
                        var chat = _client.GetChatAsync(chatId: channel.ChatId).Result;

                        forwardInfo = chat.Title;

                        if (channel.AuthorSignature != string.Empty)
                        {
                            forwardInfo = forwardInfo + $" ({channel.AuthorSignature})";
                        }

                        TextBlockForwardInfo.Text = $"Forwarded from {forwardInfo}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginChat chat:
                    {
                        TextBlockForwardInfo.Text = $"Forwarded from {chat.AuthorSignature}";
                        TextBlockForwardInfo.Visibility = Visibility.Visible;
                        break;
                    }
                    case TdApi.MessageOrigin.MessageOriginUser user:
                    {
                        var originUser = _client.GetUserAsync(userId: user.SenderUserId).Result;
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

        try
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(message.Date).ToLocalTime();
            string sendTime = dateTime.ToShortTimeString();

            SendTime.Text = sendTime;
        }
        catch
        {
            // ignored
        }
        
        switch (_messageMediaContent)
        {
            case TdApi.MessageContent.MessagePhoto messagePhoto:
            {
                _mediaFileId = messagePhoto.Photo.Sizes[1].Photo.Id;
                
                if (messagePhoto.Photo.Sizes[1].Photo.Local.Path != string.Empty)
                {
                    Image.ImageSource = new BitmapImage(new Uri(messagePhoto.Photo.Sizes[1].Photo.Local.Path));
                    SetImageTransform(messagePhoto);
                }
                else
                {
                    _client.DownloadFileAsync(fileId: messagePhoto.Photo.Sizes[1].Photo.Id, priority: 1);
                }
                
                if (messagePhoto.Caption.Text != string.Empty)
                {
                    MessageContent.Text = messagePhoto.Caption.Text;
                    MessageContent.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageContent.Text = string.Empty;
                    MessageContent.Visibility = Visibility.Collapsed;
                }
                break;
            }
        }
        
        // var messageReactions = _client.ExecuteAsync(new TdApi.GetMessageAddedReactions
        // {
        //     ChatId = message.ChatId,
        //     MessageId = message.Id,
        //     Limit = 100,
        // }).Result;
        //
        // if (messageReactions != null)
        // {
        //     foreach (var reaction in messageReactions.Reactions)
        //     {
        //         GenerateReaction(reaction);
        //     }
        // }
    }

    private void SetImageTransform(TdApi.MessageContent.MessagePhoto messagePhoto)
    {
        BorderImage.Width = messagePhoto.Photo.Sizes[1].Width / 2;
        BorderImage.Height = messagePhoto.Photo.Sizes[1].Height / 2;
    }
    
    private void GenerateReaction(TdApi.AddedReaction reaction)
    {
        var background = new Border();
        background.CornerRadius = new CornerRadius(4);
        background.Padding = new Thickness(5);
        background.BorderBrush = new SolidColorBrush(Colors.Black);

        switch (reaction.Type)
        {
            case TdApi.ReactionType.ReactionTypeEmoji emoji:
            {
                var text = new TextBlock();
                text.Text = emoji.Emoji;
                background.Child = text;
                break;
            }
            case TdApi.ReactionType.ReactionTypeCustomEmoji customEmoji:
            {
                break;
            }
        }
            
        StackPanelReactions.Children.Add(background);
    }
}