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
        /// Completely deletes a sticker set
        /// </summary>
        public class DeleteStickerSet : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "deleteStickerSet";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Sticker set name. The sticker set must be owned by the current user
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Completely deletes a sticker set
        /// </summary>
        public static Task<Ok> DeleteStickerSetAsync(
            this Client client, string name = default)
        {
            return client.ExecuteAsync(new DeleteStickerSet
            {
                Name = name
            });
        }
    }
}
// REUSE-IgnoreEnd