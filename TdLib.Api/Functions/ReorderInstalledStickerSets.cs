using System;
using System.Threading.Tasks;
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
        /// Changes the order of installed sticker sets
        /// </summary>
        public class ReorderInstalledStickerSets : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "reorderInstalledStickerSets";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Type of the sticker sets to reorder
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("sticker_type")]
            public StickerType StickerType { get; set; }

            /// <summary>
            /// Identifiers of installed sticker sets in the new correct order
            /// </summary>
            [JsonProperty("sticker_set_ids", ItemConverterType = typeof(Converter))]
            public long[] StickerSetIds { get; set; }
        }

        /// <summary>
        /// Changes the order of installed sticker sets
        /// </summary>
        public static Task<Ok> ReorderInstalledStickerSetsAsync(
            this Client client, StickerType stickerType = default, long[] stickerSetIds = default)
        {
            return client.ExecuteAsync(new ReorderInstalledStickerSets
            {
                StickerType = stickerType, StickerSetIds = stickerSetIds
            });
        }
    }
}
// REUSE-IgnoreEnd