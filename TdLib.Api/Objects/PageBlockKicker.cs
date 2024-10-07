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
            /// A kicker
            /// </summary>
            public class PageBlockKicker : PageBlock
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "pageBlockKicker";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Kicker
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("kicker")]
                public RichText Kicker { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd