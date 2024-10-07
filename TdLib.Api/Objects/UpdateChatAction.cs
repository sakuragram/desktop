using System;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        public partial class Update : Object
        {
            /// <summary>
            /// A message sender activity in the chat has changed
            /// </summary>
            public class UpdateChatAction : Update
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "updateChatAction";

                /// <summary>
                /// Extra data attached to the message
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
                /// If not 0, the message thread identifier in which the action was performed
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("message_thread_id")]
                public long MessageThreadId { get; set; }

                /// <summary>
                /// Identifier of a message sender performing the action
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("sender_id")]
                public MessageSender SenderId { get; set; }

                /// <summary>
                /// The action
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("action")]
                public ChatAction Action { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd