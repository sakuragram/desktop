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
            /// Represents a link to an MP3 audio file
            /// </summary>
            public class InputInlineQueryResultAudio : InputInlineQueryResult
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputInlineQueryResultAudio";

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
                /// Title of the audio file
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("title")]
                public string Title { get; set; }

                /// <summary>
                /// Performer of the audio file
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("performer")]
                public string Performer { get; set; }

                /// <summary>
                /// The URL of the audio file
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("audio_url")]
                public string AudioUrl { get; set; }

                /// <summary>
                /// Audio file duration, in seconds
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("audio_duration")]
                public int AudioDuration { get; set; }

                /// <summary>
                /// The message reply markup; pass null if none. Must be of type replyMarkupInlineKeyboard or null
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("reply_markup")]
                public ReplyMarkup ReplyMarkup { get; set; }

                /// <summary>
                /// The content of the message to be sent. Must be one of the following types: inputMessageText, inputMessageAudio, inputMessageInvoice, inputMessageLocation, inputMessageVenue or inputMessageContact
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("input_message_content")]
                public InputMessageContent InputMessageContent { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd