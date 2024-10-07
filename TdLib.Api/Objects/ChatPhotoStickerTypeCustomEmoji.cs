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
        public partial class ChatPhotoStickerType : Object
        {
            /// <summary>
            /// Information about the custom emoji, which was used to create the chat photo
            /// </summary>
            public class ChatPhotoStickerTypeCustomEmoji : ChatPhotoStickerType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "chatPhotoStickerTypeCustomEmoji";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Identifier of the custom emoji
                /// </summary>
                [JsonConverter(typeof(Converter.Int64))]
                [JsonProperty("custom_emoji_id")]
                public long CustomEmojiId { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd