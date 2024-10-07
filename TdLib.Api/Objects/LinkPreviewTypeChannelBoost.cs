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
        public partial class LinkPreviewType : Object
        {
            /// <summary>
            /// The link is a link to boost a channel chat
            /// </summary>
            public class LinkPreviewTypeChannelBoost : LinkPreviewType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "linkPreviewTypeChannelBoost";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Photo of the chat; may be null
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("photo")]
                public ChatPhoto Photo { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd