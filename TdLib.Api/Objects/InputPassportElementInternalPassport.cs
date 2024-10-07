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
        public partial class InputPassportElement : Object
        {
            /// <summary>
            /// A Telegram Passport element to be saved containing the user's internal passport
            /// </summary>
            public class InputPassportElementInternalPassport : InputPassportElement
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "inputPassportElementInternalPassport";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The internal passport to be saved
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("internal_passport")]
                public InputIdentityDocument InternalPassport { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd