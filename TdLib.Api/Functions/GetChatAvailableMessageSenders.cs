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
        /// Returns the list of message sender identifiers, which can be used to send messages in a chat
        /// </summary>
        public class GetChatAvailableMessageSenders : Function<ChatMessageSenders>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getChatAvailableMessageSenders";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Chat identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }
        }

        /// <summary>
        /// Returns the list of message sender identifiers, which can be used to send messages in a chat
        /// </summary>
        public static Task<ChatMessageSenders> GetChatAvailableMessageSendersAsync(
            this Client client, long chatId = default)
        {
            return client.ExecuteAsync(new GetChatAvailableMessageSenders
            {
                ChatId = chatId
            });
        }
    }
}
// REUSE-IgnoreEnd