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
        /// <summary>
        /// Contains information about an unread reaction to a message
        /// </summary>
        public partial class UnreadReaction : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "unreadReaction";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Type of the reaction
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("type")]
            public ReactionType Type { get; set; }

            /// <summary>
            /// Identifier of the sender, added the reaction
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("sender_id")]
            public MessageSender SenderId { get; set; }

            /// <summary>
            /// True, if the reaction was added with a big animation
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_big")]
            public bool IsBig { get; set; }
        }
    }
}
// REUSE-IgnoreEnd