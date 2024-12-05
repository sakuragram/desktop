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
        /// Represents short information about a sticker set
        /// </summary>
        public partial class StickerSetInfo : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "stickerSetInfo";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the sticker set
            /// </summary>
            [JsonConverter(typeof(Converter.Int64))]
            [JsonProperty("id")]
            public long Id { get; set; }

            /// <summary>
            /// Title of the sticker set
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("title")]
            public string Title { get; set; }

            /// <summary>
            /// Name of the sticker set
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Sticker set thumbnail in WEBP, TGS, or WEBM format with width and height 100; may be null. The file can be downloaded only before the thumbnail is changed
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("thumbnail")]
            public Thumbnail Thumbnail { get; set; }

            /// <summary>
            /// Sticker set thumbnail's outline; may be null if unknown
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("thumbnail_outline")]
            public Outline ThumbnailOutline { get; set; }

            /// <summary>
            /// True, if the sticker set is owned by the current user
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_owned")]
            public bool IsOwned { get; set; }

            /// <summary>
            /// True, if the sticker set has been installed by the current user
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_installed")]
            public bool IsInstalled { get; set; }

            /// <summary>
            /// True, if the sticker set has been archived. A sticker set can't be installed and archived simultaneously
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_archived")]
            public bool IsArchived { get; set; }

            /// <summary>
            /// True, if the sticker set is official
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_official")]
            public bool IsOfficial { get; set; }

            /// <summary>
            /// Type of the stickers in the set
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("sticker_type")]
            public StickerType StickerType { get; set; }

            /// <summary>
            /// True, if stickers in the sticker set are custom emoji that must be repainted; for custom emoji sticker sets only
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("needs_repainting")]
            public bool NeedsRepainting { get; set; }

            /// <summary>
            /// True, if stickers in the sticker set are custom emoji that can be used as chat emoji status; for custom emoji sticker sets only
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_allowed_as_chat_emoji_status")]
            public bool IsAllowedAsChatEmojiStatus { get; set; }

            /// <summary>
            /// True for already viewed trending sticker sets
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_viewed")]
            public bool IsViewed { get; set; }

            /// <summary>
            /// Total number of stickers in the set
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("size")]
            public int Size { get; set; }

            /// <summary>
            /// Up to the first 5 stickers from the set, depending on the context. If the application needs more stickers the full sticker set needs to be requested
            /// </summary>
            [JsonProperty("covers", ItemConverterType = typeof(Converter))]
            public Sticker[] Covers { get; set; }
        }
    }
}
// REUSE-IgnoreEnd