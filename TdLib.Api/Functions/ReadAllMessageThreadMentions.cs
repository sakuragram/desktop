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
        /// Marks all mentions in a forum topic as read
        /// </summary>
        public class ReadAllMessageThreadMentions : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "readAllMessageThreadMentions";

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
            /// Message thread identifier in which mentions are marked as read
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_thread_id")]
            public long MessageThreadId { get; set; }
        }

        /// <summary>
        /// Marks all mentions in a forum topic as read
        /// </summary>
        public static Task<Ok> ReadAllMessageThreadMentionsAsync(
            this Client client, long chatId = default, long messageThreadId = default)
        {
            return client.ExecuteAsync(new ReadAllMessageThreadMentions
            {
                ChatId = chatId, MessageThreadId = messageThreadId
            });
        }
    }
}
// REUSE-IgnoreEnd