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
        /// Returns outline of a sticker; this is an offline request. Returns a 404 error if the outline isn't known
        /// </summary>
        public class GetStickerOutline : Function<Outline>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getStickerOutline";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// File identifier of the sticker
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("sticker_file_id")]
            public int StickerFileId { get; set; }

            /// <summary>
            /// Pass true to get the outline scaled for animated emoji
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("for_animated_emoji")]
            public bool ForAnimatedEmoji { get; set; }

            /// <summary>
            /// Pass true to get the outline scaled for clicked animated emoji message
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("for_clicked_animated_emoji_message")]
            public bool ForClickedAnimatedEmojiMessage { get; set; }
        }

        /// <summary>
        /// Returns outline of a sticker; this is an offline request. Returns a 404 error if the outline isn't known
        /// </summary>
        public static Task<Outline> GetStickerOutlineAsync(
            this Client client, int stickerFileId = default, bool forAnimatedEmoji = default, bool forClickedAnimatedEmojiMessage = default)
        {
            return client.ExecuteAsync(new GetStickerOutline
            {
                StickerFileId = stickerFileId, ForAnimatedEmoji = forAnimatedEmoji, ForClickedAnimatedEmojiMessage = forClickedAnimatedEmojiMessage
            });
        }
    }
}
// REUSE-IgnoreEnd