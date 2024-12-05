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
        /// Describes a possibly non-integer amount of Telegram Stars
        /// </summary>
        public partial class StarAmount : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "starAmount";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The integer amount of Telegram Stars rounded to 0
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("star_count")]
            public long StarCount { get; set; }

            /// <summary>
            /// The number of 1/1000000000 shares of Telegram Stars; from -999999999 to 999999999
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("nanostar_count")]
            public int NanostarCount { get; set; }
        }
    }
}
// REUSE-IgnoreEnd