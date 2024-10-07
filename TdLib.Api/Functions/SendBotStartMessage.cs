using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        /// <summary>
        /// Invites a bot to a chat (if it is not yet a member) and sends it the /start command; requires can_invite_users member right. Bots can't be invited to a private chat other than the chat with the bot.
        /// Bots can't be invited to channels (although they can be added as admins) and secret chats. Returns the sent message
        /// </summary>
        public class SendBotStartMessage : Function<Message>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "sendBotStartMessage";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the bot
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("bot_user_id")]
            public long BotUserId { get; set; }

            /// <summary>
            /// Identifier of the target chat
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }

            /// <summary>
            /// A hidden parameter sent to the bot for deep linking purposes (https://core.telegram.org/bots#deep-linking)
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("parameter")]
            public string Parameter { get; set; }
        }

        /// <summary>
        /// Invites a bot to a chat (if it is not yet a member) and sends it the /start command; requires can_invite_users member right. Bots can't be invited to a private chat other than the chat with the bot.
        /// Bots can't be invited to channels (although they can be added as admins) and secret chats. Returns the sent message
        /// </summary>
        public static Task<Message> SendBotStartMessageAsync(
            this Client client, long botUserId = default, long chatId = default, string parameter = default)
        {
            return client.ExecuteAsync(new SendBotStartMessage
            {
                BotUserId = botUserId, ChatId = chatId, Parameter = parameter
            });
        }
    }
}
// REUSE-IgnoreEnd