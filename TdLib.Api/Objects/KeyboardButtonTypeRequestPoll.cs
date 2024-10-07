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
        public partial class KeyboardButtonType : Object
        {
            /// <summary>
            /// A button that allows the user to create and send a poll when pressed; available only in private chats
            /// </summary>
            public class KeyboardButtonTypeRequestPoll : KeyboardButtonType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "keyboardButtonTypeRequestPoll";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// If true, only regular polls must be allowed to create
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("force_regular")]
                public bool ForceRegular { get; set; }

                /// <summary>
                /// If true, only polls in quiz mode must be allowed to create
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("force_quiz")]
                public bool ForceQuiz { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd