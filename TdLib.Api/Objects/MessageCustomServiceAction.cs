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
        public partial class MessageContent : Object
        {
            /// <summary>
            /// A non-standard action has happened in the chat
            /// </summary>
            public class MessageCustomServiceAction : MessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageCustomServiceAction";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Message text to be shown in the chat
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("text")]
                public string Text { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd