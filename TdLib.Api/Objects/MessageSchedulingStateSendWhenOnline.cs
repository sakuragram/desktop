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
        public partial class MessageSchedulingState : Object
        {
            /// <summary>
            /// The message will be sent when the other user is online. Applicable to private chats only and when the exact online status of the other user is known
            /// </summary>
            public class MessageSchedulingStateSendWhenOnline : MessageSchedulingState
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageSchedulingStateSendWhenOnline";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }


            }
        }
    }
}
// REUSE-IgnoreEnd