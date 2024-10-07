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
        /// Changes the pinned state of a forum topic; requires can_manage_topics right in the supergroup. There can be up to getOption("pinned_forum_topic_count_max") pinned forum topics
        /// </summary>
        public class ToggleForumTopicIsPinned : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "toggleForumTopicIsPinned";

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
            /// Message thread identifier of the forum topic
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_thread_id")]
            public long MessageThreadId { get; set; }

            /// <summary>
            /// Pass true to pin the topic; pass false to unpin it
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_pinned")]
            public bool IsPinned { get; set; }
        }

        /// <summary>
        /// Changes the pinned state of a forum topic; requires can_manage_topics right in the supergroup. There can be up to getOption("pinned_forum_topic_count_max") pinned forum topics
        /// </summary>
        public static Task<Ok> ToggleForumTopicIsPinnedAsync(
            this Client client, long chatId = default, long messageThreadId = default, bool isPinned = default)
        {
            return client.ExecuteAsync(new ToggleForumTopicIsPinned
            {
                ChatId = chatId, MessageThreadId = messageThreadId, IsPinned = isPinned
            });
        }
    }
}
// REUSE-IgnoreEnd