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
        public partial class InputInlineQueryResult : Object
        {
            /// <summary>
            /// Represents a link to a WEBP, TGS, or WEBM sticker
            /// </summary>
            public class InputInlineQueryResultSticker : InputInlineQueryResult
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputInlineQueryResultSticker";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Unique identifier of the query result
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("id")]
                public string Id { get; set; }

                /// <summary>
                /// URL of the sticker thumbnail, if it exists
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("thumbnail_url")]
                public string ThumbnailUrl { get; set; }

                /// <summary>
                /// The URL of the WEBP, TGS, or WEBM sticker (sticker file size must not exceed 5MB)
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("sticker_url")]
                public string StickerUrl { get; set; }

                /// <summary>
                /// Width of the sticker
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("sticker_width")]
                public int StickerWidth { get; set; }

                /// <summary>
                /// Height of the sticker
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("sticker_height")]
                public int StickerHeight { get; set; }

                /// <summary>
                /// The message reply markup; pass null if none. Must be of type replyMarkupInlineKeyboard or null
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("reply_markup")]
                public ReplyMarkup ReplyMarkup { get; set; }

                /// <summary>
                /// The content of the message to be sent. Must be one of the following types: inputMessageText, inputMessageSticker, inputMessageInvoice, inputMessageLocation, inputMessageVenue or inputMessageContact
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("input_message_content")]
                public InputMessageContent InputMessageContent { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd