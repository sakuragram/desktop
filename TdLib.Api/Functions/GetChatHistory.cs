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
        /// Returns messages in a chat. The messages are returned in reverse chronological order (i.e., in order of decreasing message_id).
        /// For optimal performance, the number of returned messages is chosen by TDLib. This is an offline request if only_local is true
        /// </summary>
        public class GetChatHistory : Function<Messages>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getChatHistory";

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

            /// <summary>
            /// Identifier of the message starting from which history must be fetched; use 0 to get results from the last message
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("from_message_id")]
            public long FromMessageId { get; set; }

            /// <summary>
            /// Specify 0 to get results from exactly the message from_message_id or a negative offset up to 99 to get additionally some newer messages
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("offset")]
            public int Offset { get; set; }

            /// <summary>
            /// The maximum number of messages to be returned; must be positive and can't be greater than 100. If the offset is negative, the limit must be greater than or equal to -offset.
            /// For optimal performance, the number of returned messages is chosen by TDLib and can be smaller than the specified limit
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("limit")]
            public int Limit { get; set; }

            /// <summary>
            /// Pass true to get only messages that are available without sending network requests
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("only_local")]
            public bool OnlyLocal { get; set; }
        }

        /// <summary>
        /// Returns messages in a chat. The messages are returned in reverse chronological order (i.e., in order of decreasing message_id).
        /// For optimal performance, the number of returned messages is chosen by TDLib. This is an offline request if only_local is true
        /// </summary>
        public static Task<Messages> GetChatHistoryAsync(
            this Client client, long chatId = default, long fromMessageId = default, int offset = default, int limit = default, bool onlyLocal = default)
        {
            return client.ExecuteAsync(new GetChatHistory
            {
                ChatId = chatId, FromMessageId = fromMessageId, Offset = offset, Limit = limit, OnlyLocal = onlyLocal
            });
        }
    }
}
// REUSE-IgnoreEnd