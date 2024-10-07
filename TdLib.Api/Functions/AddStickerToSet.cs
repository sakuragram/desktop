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
        /// Adds a new sticker to a set
        /// </summary>
        public class AddStickerToSet : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "addStickerToSet";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Sticker set owner; ignored for regular users
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("user_id")]
            public long UserId { get; set; }

            /// <summary>
            /// Sticker set name. The sticker set must be owned by the current user, and contain less than 200 stickers for custom emoji sticker sets and less than 120 otherwise
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Sticker to add to the set
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("sticker")]
            public InputSticker Sticker { get; set; }
        }

        /// <summary>
        /// Adds a new sticker to a set
        /// </summary>
        public static Task<Ok> AddStickerToSetAsync(
            this Client client, long userId = default, string name = default, InputSticker sticker = default)
        {
            return client.ExecuteAsync(new AddStickerToSet
            {
                UserId = userId, Name = name, Sticker = sticker
            });
        }
    }
}
// REUSE-IgnoreEnd