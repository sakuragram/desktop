using System;
using Microsoft.UI.Xaml.Controls;
using sakuragram.Services.Core;
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
                TdApi.MessageContent.MessageBasicGroupChatCreate messageBasicGroupChatCreate => $"{senderName} created the group «{messageBasicGroupChatCreate.Title}»",
                TdApi.MessageContent.MessageBotWriteAccessAllowed messageBotWriteAccessAllowed => messageBotWriteAccessAllowed.Reason.ToString(),
                TdApi.MessageContent.MessageChatAddMembers messageChatAddMembers => $"{senderName} added {messageChatAddMembers.MemberUserIds} members",
                TdApi.MessageContent.MessageChatBoost messageChatBoostText => $"{senderName} boosted this chat {messageChatBoostText.BoostCount}",
                TdApi.MessageContent.MessageGameScore messageGameScoreText => $"{senderName} scored {messageGameScoreText.Score} in game",
                TdApi.MessageContent.MessageGiftedPremium messageGiftedPremiumText => 
                    $"{senderName} received a {messageGiftedPremiumText.MonthCount}-month premium for {messageGiftedPremiumText.Amount} {messageGiftedPremiumText.Currency}",
                TdApi.MessageContent.MessageInviteVideoChatParticipants messageInviteVideoChatParticipants => 
                    $"{senderName} invited {messageInviteVideoChatParticipants.UserIds} to the video chat",
                TdApi.MessageContent.MessagePassportDataReceived messagePassportDataReceived => "messagePassportDataReceived",
                TdApi.MessageContent.MessagePassportDataSent messagePassportDataSent => "messagePassportDataSent",
                TdApi.MessageContent.MessageProximityAlertTriggered messageProximityAlertTriggered => "messageProximityAlertTriggered",
                TdApi.MessageContent.MessageScreenshotTaken messageScreenshotTaken => "messageScreenshotTaken",
                TdApi.MessageContent.MessageSupergroupChatCreate messageSupergroupChatCreate => $"Created supergroup.",
                TdApi.MessageContent.MessageVideoChatEnded messageVideoChatEnded => "Video chat ended",
                TdApi.MessageContent.MessageVideoChatScheduled messageVideoChatScheduled => 
                    $"Video chat scheduled to {MathService.CalculateDateTime(messageVideoChatScheduled.StartDate).ToShortTimeString()}",
                TdApi.MessageContent.MessageVideoChatStarted messageVideoChatStarted => "Video chat started",
                TdApi.MessageContent.MessageChatChangeTitle messageChatChangeTitleText => $"{senderName} changed chat title to «{messageChatChangeTitleText.Title}»",
                TdApi.MessageContent.MessageChatDeleteMember messageChatDeleteMember => $"{senderName} removed {messageChatDeleteMember.UserId} from the chat",
                TdApi.MessageContent.MessageChatDeletePhoto messageChatDeletePhoto => "messageChatDeletePhoto",
                TdApi.MessageContent.MessageChatJoinByLink messageChatJoinByLink => $"{senderName} joined the chat by link",
                TdApi.MessageContent.MessageChatJoinByRequest messageChatJoinByRequest => $"{senderName} joined the chat by request",
                TdApi.MessageContent.MessageChatSetMessageAutoDeleteTime messageChatSetMessageAutoDeleteTime => 
                    $"{senderName} {messageChatSetMessageAutoDeleteTime.MessageAutoDeleteTime}",
                TdApi.MessageContent.MessageChatUpgradeFrom messageChatUpgradeFrom => "messageChatUpgradeFrom",
                TdApi.MessageContent.MessageChatUpgradeTo messageChatUpgradeTo => "messageChatUpgradeTo",
                TdApi.MessageContent.MessageContactRegistered messageContactRegistered => $"{senderName} registered contact",
                TdApi.MessageContent.MessageCustomServiceAction messageCustomServiceAction => messageCustomServiceAction.Text,
                TdApi.MessageContent.MessageForumTopicCreated messageForumTopicCreated => $"{senderName} created the topic «{messageForumTopicCreated.Name}»",
                TdApi.MessageContent.MessageForumTopicEdited messageForumTopicEdited => $"{senderName} edited the topic «{messageForumTopicEdited.Name}»",
                TdApi.MessageContent.MessageForumTopicIsClosedToggled messageForumTopicIsClosedToggled => "messageForumTopicIsClosedToggled",
                TdApi.MessageContent.MessageForumTopicIsHiddenToggled messageForumTopicIsHiddenToggled => "messageForumTopicIsHiddenToggled",
                _ => "Unsupported message type"
            };
        }

        public void UpdateDateMessage(DateTime newDate = default, DateTime oldDate = default)
        {
            MessageText.Text = newDate.ToString(newDate.Year > oldDate.Year ? "M yyyy" : "M");
        }
    }
}
