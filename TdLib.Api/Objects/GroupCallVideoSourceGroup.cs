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
        /// Describes a group of video synchronization source identifiers
        /// </summary>
        public partial class GroupCallVideoSourceGroup : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "groupCallVideoSourceGroup";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The semantics of sources, one of "SIM" or "FID"
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("semantics")]
            public string Semantics { get; set; }

            /// <summary>
            /// The list of synchronization source identifiers
            /// </summary>
            [JsonProperty("source_ids", ItemConverterType = typeof(Converter))]
            public int[] SourceIds { get; set; }
        }
    }
}
// REUSE-IgnoreEnd