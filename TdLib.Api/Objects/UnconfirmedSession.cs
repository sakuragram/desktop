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
        /// Contains information about an unconfirmed session
        /// </summary>
        public partial class UnconfirmedSession : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "unconfirmedSession";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Session identifier
            /// </summary>
            [JsonConverter(typeof(Converter.Int64))]
            [JsonProperty("id")]
            public long Id { get; set; }

            /// <summary>
            /// Point in time (Unix timestamp) when the user has logged in
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("log_in_date")]
            public int LogInDate { get; set; }

            /// <summary>
            /// Model of the device that was used for the session creation, as provided by the application
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("device_model")]
            public string DeviceModel { get; set; }

            /// <summary>
            /// A human-readable description of the location from which the session was created, based on the IP address
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("location")]
            public string Location { get; set; }
        }
    }
}
// REUSE-IgnoreEnd