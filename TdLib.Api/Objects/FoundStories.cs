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
        /// Contains a list of stories found by a search
        /// </summary>
        public partial class FoundStories : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "foundStories";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Approximate total number of stories found
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("total_count")]
            public int TotalCount { get; set; }

            /// <summary>
            /// List of stories
            /// </summary>
            [JsonProperty("stories", ItemConverterType = typeof(Converter))]
            public Story[] Stories { get; set; }

            /// <summary>
            /// The offset for the next request. If empty, then there are no more results
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("next_offset")]
            public string NextOffset { get; set; }
        }
    }
}
// REUSE-IgnoreEnd