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
        /// Contains a value representing a number of seconds
        /// </summary>
        public partial class Seconds : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "seconds";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Number of seconds
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("seconds")]
            public double? Seconds_ { get; set; }
        }
    }
}
// REUSE-IgnoreEnd