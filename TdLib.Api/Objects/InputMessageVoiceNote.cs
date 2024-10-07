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
            /// A voice note message
            /// </summary>
            public class InputMessageVoiceNote : InputMessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputMessageVoiceNote";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Voice note to be sent. The voice note must be encoded with the Opus codec and stored inside an OGG container with a single audio channel, or be in MP3 or M4A format as regular audio
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("voice_note")]
                public InputFile VoiceNote { get; set; }

                /// <summary>
                /// Duration of the voice note, in seconds
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("duration")]
                public int Duration { get; set; }

                /// <summary>
                /// Waveform representation of the voice note in 5-bit format
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("waveform")]
                public byte[] Waveform { get; set; }

                /// <summary>
                /// Voice note caption; may be null if empty; pass null to use an empty caption; 0-getOption("message_caption_length_max") characters
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("caption")]
                public FormattedText Caption { get; set; }

                /// <summary>
                /// Voice note self-destruct type; may be null if none; pass null if none; private chats only
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("self_destruct_type")]
                public MessageSelfDestructType SelfDestructType { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd