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
        public partial class InlineKeyboardButtonType : Object
        {
            /// <summary>
            /// A button that copies specified text to clipboard
            /// </summary>
            public class InlineKeyboardButtonTypeCopyText : InlineKeyboardButtonType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inlineKeyboardButtonTypeCopyText";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The text to copy to clipboard
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("text")]
                public string Text { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd