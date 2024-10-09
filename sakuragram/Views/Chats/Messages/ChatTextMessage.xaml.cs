using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Views.Chats.Messages
{
    public sealed partial class ChatTextMessage : Page
    {
        private static TdClient _client = App._client;
        private bool _isContextMenuOpen = false;
        
        private long _chatId;
        public long _messageId;
        private TdApi.ProfilePhoto _profilePhoto;

        public ReplyService _replyService;
        public MessageService _messageService;
        private MediaService _mediaService = new();

        private bool _bIsSelected = false;

        public ChatTextMessage()
        {
            InitializeComponent();
        }

        private Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateMessageEdited:
                {
                    var message = _client.ExecuteAsync(new TdApi.GetMessage
                    {
                        ChatId = _chatId,
                        MessageId = _messageId
                    });

                    MessageContent.Text = message.Result.Content switch
                    {
                        TdApi.MessageContent.MessageText messageText => MessageContent.Text = messageText.Text.Text,
                        _ => MessageContent.Text
                    };
                    break;
                }
            }
            
            return Task.CompletedTask;
        }

        public void UpdateMessage(TdApi.Message message)
        {
            _chatId = message.ChatId;
            _messageId = message.Id;
            
            if (message.ReplyTo != null)
            {
                var replyMessage = _client.ExecuteAsync(new TdApi.GetRepliedMessage
                {
                    ChatId = message.ChatId,
                    MessageId = _messageId
                }).Result;
                
                var replyUserId = replyMessage.SenderId switch {
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
                
                ReplyInputContent.Text  = replyMessage.Content switch
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
            
            try
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(message.Date).ToLocalTime();
                string sendTime = dateTime.ToShortTimeString();

                TextBlockSendTime.Text = sendTime;
            } 
            catch 
            {
                // ignored
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
            
            TextBlockEdited.Visibility = message.EditDate != 0 ? Visibility.Visible : Visibility.Collapsed;

            if (message.InteractionInfo?.ViewCount > 0 && message.IsChannelPost)
            {
                TextBlockViews.Text = message.InteractionInfo.ViewCount + " views";
                TextBlockViews.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockViews.Text = string.Empty;
                TextBlockViews.Visibility = Visibility.Collapsed;
            }

            if (message.InteractionInfo?.ReplyInfo != null)
            {
                if (message.InteractionInfo.ReplyInfo.ReplyCount > 0)
                {
                    TextBlockReplies.Text = message.InteractionInfo.ReplyInfo.ReplyCount + " replies";
                    TextBlockReplies.Visibility = Visibility.Visible;
                }
                else
                {
                    TextBlockReplies.Text = string.Empty;
                    TextBlockReplies.Visibility = Visibility.Collapsed;
                }
            }
            
            switch (message.Content)
            {
                case TdApi.MessageContent.MessageText messageText:
                    var text = messageText.Text.Text;
                    var regex = new Regex(@"(https?://[^\s]+)");
                    var match = regex.Match(text);

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
                        MessageContent.Inlines.Add(new Run { Text = text.Substring(0, match.Index) });
                        MessageContent.Inlines.Add(hyperlink);
                        MessageContent.Inlines.Add(new Run { Text = text.Substring(match.Index + match.Length) });
                    }
                    else
                    {
                        MessageContent.Text = text;
                    }
                    break;
                case TdApi.MessageContent.MessageUnsupported:
                    MessageContent.Text = "Your version of CherryMerryGram does not support this type of message, make sure that you are using the latest version of the client.";
                    break;
                default:
                    MessageContent.Text = "Unsupported message type";
                    break;
            }
            
            // var messageReactions = _client.ExecuteAsync(new TdApi.GetMessageAddedReactions
            // {
            //     ChatId = message.ChatId,
            //     MessageId = message.Id,
            //     Limit = 100
            // }).Result;
            //
            // if (messageReactions != null)
            // {
            //     foreach (var reaction in messageReactions.Reactions)
            //     {
            //         GenerateReaction(reaction);
            //     }
            // }
            
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
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
        
        private void ShowMenu(bool isTransient)
        {
            _isContextMenuOpen = isTransient;
            FlyoutShowOptions myOption = new FlyoutShowOptions();
            myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
            CommandBarFlyout1.ShowAt(Message, myOption);
        }
        
        private void UIElement_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowMenu(!_isContextMenuOpen);
        }

        private void Reply_OnClick(object sender, RoutedEventArgs e)
        {
            _replyService.SelectMessageForReply(_messageId);
        }

        private async void Forward_OnClick(object sender, RoutedEventArgs e)
        {
            if (_messageService.GetSelectedMessages().Length == 0)
            {
                _messageService.SelectMessage(_messageId);
            }
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
            
            ShowMenu(false);
        }

        private void Select_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_bIsSelected)
            {
                _messageService.SelectMessage(messageId: _messageId);
                MessageBackground.Background = new SolidColorBrush(Colors.Gray);
                _bIsSelected = true;
            }
            else
            {
                _messageService.DeselectMessage(messageId: _messageId);
                MessageBackground.Background = new SolidColorBrush(Colors.Black);
                _bIsSelected = false;
            }
        }

        private void Report_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ChatMessage_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _replyService.SelectMessageForReply(_messageId);
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

        private void PinMessageConfirmation_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _client.PinChatMessageAsync(_chatId, _messageId, NotifyAllMembers.IsChecked.Value);
        }

        private void DeleteMessagesConfirmation_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_messageService._isMessageSelected)
            {
                _client.ExecuteAsync(new TdApi.DeleteMessages
                {
                    ChatId = _chatId,
                    MessageIds = _messageService.GetSelectedMessages(),
                    Revoke = Revoke.IsChecked.Value
                });
            }
            else
            {
                _client.ExecuteAsync(new TdApi.DeleteMessages
                {
                    ChatId = _chatId,
                    MessageIds = new long[] { _messageId },
                    Revoke = Revoke.IsChecked.Value
                });
            }
        }

        private void ChatMessage_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_messageService._isMessageSelected) Select_OnClick(null, null);
        }
    }
}
