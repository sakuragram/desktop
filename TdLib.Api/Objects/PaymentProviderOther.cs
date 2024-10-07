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
        public partial class PaymentProvider : Object
        {
            /// <summary>
            /// Some other payment provider, for which a web payment form must be shown
            /// </summary>
            public class PaymentProviderOther : PaymentProvider
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "paymentProviderOther";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Payment form URL
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("url")]
                public string Url { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd