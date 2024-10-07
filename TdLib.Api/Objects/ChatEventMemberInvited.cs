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
        public partial class ChatEventAction : Object
        {
            /// <summary>
            /// A new chat member was invited
            /// </summary>
            public class ChatEventMemberInvited : ChatEventAction
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "chatEventMemberInvited";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// New member user identifier
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("user_id")]
                public long UserId { get; set; }

                /// <summary>
                /// New member status
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("status")]
                public ChatMemberStatus Status { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd