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
        /// Contains the description of an error in a Telegram Passport element
        /// </summary>
        public partial class PassportElementError : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "passportElementError";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Type of the Telegram Passport element which has the error
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("type")]
            public PassportElementType Type { get; set; }

            /// <summary>
            /// Error message
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message")]
            public string Message { get; set; }

            /// <summary>
            /// Error source
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("source")]
            public PassportElementErrorSource Source { get; set; }
        }
    }
}
// REUSE-IgnoreEnd