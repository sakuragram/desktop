using System;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace sakuragram.Views.Chats.Messages
{
    public partial class ChatServiceMessage : Page
    {
        private static TdClient _client = App._client;
        
        public ChatServiceMessage()
        {
            InitializeComponent();
        }

        public void UpdateMessage(TdApi.Message message)
        {
            string senderName = string.Empty;
            
            var sender = message.SenderId switch
            {
                TdApi.MessageSender.MessageSenderUser u => u.UserId,
                TdApi.MessageSender.MessageSenderChat c => c.ChatId,
                _ => 0
            };
            
            if (sender > 0)
            {
                var user = _client.GetUserAsync(sender).Result;
                senderName = user.FirstName;
            }
            else
            {
                var chat = _client.GetChatAsync(message.ChatId).Result;
                senderName = chat.Title;
            }
            
            MessageText.Text = message.Content switch
            {
                TdApi.MessageContent.MessageChatBoost messageChatBoostText => $"{senderName} boosted this chat {messageChatBoostText.BoostCount}",
                TdApi.MessageContent.MessageGameScore messageGameScoreText => $"{senderName} scored {messageGameScoreText.Score} in game",
                TdApi.MessageContent.MessageGiftedPremium messageGiftedPremiumText => 
                    $"{senderName} received a {messageGiftedPremiumText.MonthCount}-month premium for {messageGiftedPremiumText.Amount} {messageGiftedPremiumText.Currency}",
                TdApi.MessageContent.MessagePinMessage messagePinMessageText => $"{senderName} pinned this message ({messagePinMessageText.MessageId})",
                TdApi.MessageContent.MessageChatChangeTitle messageChatChangeTitleText => $"{senderName} changed chat title to «{messageChatChangeTitleText.Title}»",
                _ => "Unsupported message type"
            };
        }

        public void UpdateDateMessage(DateTime newDate = default, DateTime oldDate = default)
        {
            MessageText.Text = newDate.ToString(newDate.Year > oldDate.Year ? "M yyyy" : "M");
        }
    }
}
