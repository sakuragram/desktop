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
        /// Thumbnail image of a very poor quality and low resolution
        /// </summary>
        public partial class Minithumbnail : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "minithumbnail";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Thumbnail width, usually doesn't exceed 40
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("width")]
            public int Width { get; set; }

            /// <summary>
            /// Thumbnail height, usually doesn't exceed 40
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("height")]
            public int Height { get; set; }

            /// <summary>
            /// The thumbnail in JPEG format
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("data")]
            public byte[] Data { get; set; }
        }
    }
}
// REUSE-IgnoreEnd