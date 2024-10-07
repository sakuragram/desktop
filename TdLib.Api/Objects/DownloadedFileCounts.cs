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
        /// Contains number of being downloaded and recently downloaded files found
        /// </summary>
        public partial class DownloadedFileCounts : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "downloadedFileCounts";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Number of active file downloads found, including paused
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("active_count")]
            public int ActiveCount { get; set; }

            /// <summary>
            /// Number of paused file downloads found
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("paused_count")]
            public int PausedCount { get; set; }

            /// <summary>
            /// Number of completed file downloads found
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("completed_count")]
            public int CompletedCount { get; set; }
        }
    }
}
// REUSE-IgnoreEnd