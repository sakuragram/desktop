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
        public partial class InputMessageContent : Object
        {
            /// <summary>
            /// A video message
            /// </summary>
            public class InputMessageVideo : InputMessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputMessageVideo";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Video to be sent. The video is expected to be reencoded to MPEG4 format with H.264 codec by the sender
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("video")]
                public InputFile Video { get; set; }

                /// <summary>
                /// Video thumbnail; pass null to skip thumbnail uploading
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("thumbnail")]
                public InputThumbnail Thumbnail { get; set; }

                /// <summary>
                /// File identifiers of the stickers added to the video, if applicable
                /// </summary>
                [JsonProperty("added_sticker_file_ids", ItemConverterType = typeof(Converter))]
                public int[] AddedStickerFileIds { get; set; }

                /// <summary>
                /// Duration of the video, in seconds
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("duration")]
                public int Duration { get; set; }

                /// <summary>
                /// Video width
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("width")]
                public int Width { get; set; }

                /// <summary>
                /// Video height
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("height")]
                public int Height { get; set; }

                /// <summary>
                /// True, if the video is expected to be streamed
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("supports_streaming")]
                public bool SupportsStreaming { get; set; }

                /// <summary>
                /// Video caption; pass null to use an empty caption; 0-getOption("message_caption_length_max") characters
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("caption")]
                public FormattedText Caption { get; set; }

                /// <summary>
                /// True, if the caption must be shown above the video; otherwise, the caption must be shown below the video; not supported in secret chats
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("show_caption_above_media")]
                public bool ShowCaptionAboveMedia { get; set; }

                /// <summary>
                /// Video self-destruct type; pass null if none; private chats only
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("self_destruct_type")]
                public MessageSelfDestructType SelfDestructType { get; set; }

                /// <summary>
                /// True, if the video preview must be covered by a spoiler animation; not supported in secret chats
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("has_spoiler")]
                public bool HasSpoiler { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd