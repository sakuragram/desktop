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
        public partial class Update : Object
        {
            /// <summary>
            /// The list of installed sticker sets was updated
            /// </summary>
            public class UpdateInstalledStickerSets : Update
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "updateInstalledStickerSets";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Type of the affected stickers
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("sticker_type")]
                public StickerType StickerType { get; set; }

                /// <summary>
                /// The new list of installed ordinary sticker sets
                /// </summary>
                [JsonProperty("sticker_set_ids", ItemConverterType = typeof(Converter))]
                public long[] StickerSetIds { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd