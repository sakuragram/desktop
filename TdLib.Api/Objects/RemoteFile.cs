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
        /// Represents a remote file
        /// </summary>
        public partial class RemoteFile : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "remoteFile";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Remote file identifier; may be empty. Can be used by the current user across application restarts or even from other devices. Uniquely identifies a file, but a file can have a lot of different valid identifiers.
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("id")]
            public string Id { get; set; }

            /// <summary>
            /// Unique file identifier; may be empty if unknown. The unique file identifier which is the same for the same file even for different users and is persistent over time
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("unique_id")]
            public string UniqueId { get; set; }

            /// <summary>
            /// True, if the file is currently being uploaded (or a remote copy is being generated by some other means)
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_uploading_active")]
            public bool IsUploadingActive { get; set; }

            /// <summary>
            /// True, if a remote copy is fully available
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_uploading_completed")]
            public bool IsUploadingCompleted { get; set; }

            /// <summary>
            /// Size of the remote available part of the file, in bytes; 0 if unknown
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("uploaded_size")]
            public long UploadedSize { get; set; }
        }
    }
}
// REUSE-IgnoreEnd