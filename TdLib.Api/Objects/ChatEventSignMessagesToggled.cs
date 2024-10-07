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
            /// The sign_messages setting of a channel was toggled
            /// </summary>
            public class ChatEventSignMessagesToggled : ChatEventAction
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "chatEventSignMessagesToggled";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// New value of sign_messages
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("sign_messages")]
                public bool SignMessages { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd