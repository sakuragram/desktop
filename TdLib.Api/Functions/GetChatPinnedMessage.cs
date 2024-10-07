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
        /// Returns information about a newest pinned message in the chat. Returns a 404 error if the message doesn't exist
        /// </summary>
        public class GetChatPinnedMessage : Function<Message>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getChatPinnedMessage";

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
        }

        /// <summary>
        /// Returns information about a newest pinned message in the chat. Returns a 404 error if the message doesn't exist
        /// </summary>
        public static Task<Message> GetChatPinnedMessageAsync(
            this Client client, long chatId = default)
        {
            return client.ExecuteAsync(new GetChatPinnedMessage
            {
                ChatId = chatId
            });
        }
    }
}
// REUSE-IgnoreEnd