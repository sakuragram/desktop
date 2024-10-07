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
        public partial class PageBlock : Object
        {
            /// <summary>
            /// An audio file
            /// </summary>
            public class PageBlockAudio : PageBlock
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "pageBlockAudio";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Audio file; may be null
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("audio")]
                public Audio Audio { get; set; }

                /// <summary>
                /// Audio file caption
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("caption")]
                public PageBlockCaption Caption { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd