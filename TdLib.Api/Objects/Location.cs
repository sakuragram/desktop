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
        /// Describes a location on planet Earth
        /// </summary>
        public partial class Location : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "location";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Latitude of the location in degrees; as defined by the sender
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("latitude")]
            public double? Latitude { get; set; }

            /// <summary>
            /// Longitude of the location, in degrees; as defined by the sender
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("longitude")]
            public double? Longitude { get; set; }

            /// <summary>
            /// The estimated horizontal accuracy of the location, in meters; as defined by the sender. 0 if unknown
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("horizontal_accuracy")]
            public double? HorizontalAccuracy { get; set; }
        }
    }
}
// REUSE-IgnoreEnd