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
        /// Returns information about a non-bundled message that is replied by a given message. Also, returns the pinned message, the game message, the invoice message,
        /// the message with a previously set same background, the giveaway message, and the topic creation message for messages of the types
        /// messagePinMessage, messageGameScore, messagePaymentSuccessful, messageChatSetBackground, messageGiveawayCompleted and topic messages without non-bundled replied message respectively.
        /// Returns a 404 error if the message doesn't exist
        /// </summary>
        public class GetRepliedMessage : Function<Message>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getRepliedMessage";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the chat the message belongs to
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }

            /// <summary>
            /// Identifier of the reply message
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_id")]
            public long MessageId { get; set; }
        }

        /// <summary>
        /// Returns information about a non-bundled message that is replied by a given message. Also, returns the pinned message, the game message, the invoice message,
        /// the message with a previously set same background, the giveaway message, and the topic creation message for messages of the types
        /// messagePinMessage, messageGameScore, messagePaymentSuccessful, messageChatSetBackground, messageGiveawayCompleted and topic messages without non-bundled replied message respectively.
        /// Returns a 404 error if the message doesn't exist
        /// </summary>
        public static Task<Message> GetRepliedMessageAsync(
            this Client client, long chatId = default, long messageId = default)
        {
            return client.ExecuteAsync(new GetRepliedMessage
            {
                ChatId = chatId, MessageId = messageId
            });
        }
    }
}
// REUSE-IgnoreEnd