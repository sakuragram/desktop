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
        /// Reports that authentication code wasn't delivered via SMS to the specified phone number; for official mobile applications only
        /// </summary>
        public class ReportPhoneNumberCodeMissing : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "reportPhoneNumberCodeMissing";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Current mobile network code
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("mobile_network_code")]
            public string MobileNetworkCode { get; set; }
        }

        /// <summary>
        /// Reports that authentication code wasn't delivered via SMS to the specified phone number; for official mobile applications only
        /// </summary>
        public static Task<Ok> ReportPhoneNumberCodeMissingAsync(
            this Client client, string mobileNetworkCode = default)
        {
            return client.ExecuteAsync(new ReportPhoneNumberCodeMissing
            {
                MobileNetworkCode = mobileNetworkCode
            });
        }
    }
}
// REUSE-IgnoreEnd