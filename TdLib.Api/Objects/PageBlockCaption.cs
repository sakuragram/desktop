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
        /// Contains a caption of another block
        /// </summary>
        public partial class PageBlockCaption : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "pageBlockCaption";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Content of the caption
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("text")]
            public RichText Text { get; set; }

            /// <summary>
            /// Block credit (like HTML tag &lt;cite&gt;)
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("credit")]
            public RichText Credit { get; set; }
        }
    }
}
// REUSE-IgnoreEnd