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
        public partial class ChatAction : Object
        {
            /// <summary>
            /// The user is uploading a document
            /// </summary>
            public class ChatActionUploadingDocument : ChatAction
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "chatActionUploadingDocument";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Upload progress, as a percentage
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("progress")]
                public int Progress { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd