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
        /// Describes a photo
        /// </summary>
        public partial class Photo : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "photo";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// True, if stickers were added to the photo. The list of corresponding sticker sets can be received using getAttachedStickerSets
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("has_stickers")]
            public bool HasStickers { get; set; }

            /// <summary>
            /// Photo minithumbnail; may be null
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("minithumbnail")]
            public Minithumbnail Minithumbnail { get; set; }

            /// <summary>
            /// Available variants of the photo, in different sizes
            /// </summary>
            [JsonProperty("sizes", ItemConverterType = typeof(Converter))]
            public PhotoSize[] Sizes { get; set; }
        }
    }
}
// REUSE-IgnoreEnd