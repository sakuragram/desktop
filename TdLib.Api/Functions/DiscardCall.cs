using System;
using System.Threading.Tasks;
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
        /// Discards a call
        /// </summary>
        public class DiscardCall : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "discardCall";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Call identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("call_id")]
            public int CallId { get; set; }

            /// <summary>
            /// Pass true if the user was disconnected
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_disconnected")]
            public bool IsDisconnected { get; set; }

            /// <summary>
            /// The call duration, in seconds
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("duration")]
            public int Duration { get; set; }

            /// <summary>
            /// Pass true if the call was a video call
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_video")]
            public bool IsVideo { get; set; }

            /// <summary>
            /// Identifier of the connection used during the call
            /// </summary>
            [JsonConverter(typeof(Converter.Int64))]
            [JsonProperty("connection_id")]
            public long ConnectionId { get; set; }
        }

        /// <summary>
        /// Discards a call
        /// </summary>
        public static Task<Ok> DiscardCallAsync(
            this Client client, int callId = default, bool isDisconnected = default, int duration = default, bool isVideo = default, long connectionId = default)
        {
            return client.ExecuteAsync(new DiscardCall
            {
                CallId = callId, IsDisconnected = isDisconnected, Duration = duration, IsVideo = isVideo, ConnectionId = connectionId
            });
        }
    }
}
// REUSE-IgnoreEnd