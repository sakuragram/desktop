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
        public partial class Update : Object
        {
            /// <summary>
            /// Some messages were deleted
            /// </summary>
            public class UpdateDeleteMessages : Update
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "updateDeleteMessages";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Chat identifier
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("chat_id")]
                public long ChatId { get; set; }

                /// <summary>
                /// Identifiers of the deleted messages
                /// </summary>
                [JsonProperty("message_ids", ItemConverterType = typeof(Converter))]
                public long[] MessageIds { get; set; }

                /// <summary>
                /// True, if the messages are permanently deleted by a user (as opposed to just becoming inaccessible)
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("is_permanent")]
                public bool IsPermanent { get; set; }

                /// <summary>
                /// True, if the messages are deleted only from the cache and can possibly be retrieved again in the future
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("from_cache")]
                public bool FromCache { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd