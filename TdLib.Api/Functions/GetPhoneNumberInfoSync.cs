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
        /// Returns information about a phone number by its prefix synchronously. getCountries must be called at least once after changing localization to the specified language if properly localized country information is expected. Can be called synchronously
        /// </summary>
        public class GetPhoneNumberInfoSync : Function<PhoneNumberInfo>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getPhoneNumberInfoSync";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// A two-letter ISO 639-1 language code for country information localization
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("language_code")]
            public string LanguageCode { get; set; }

            /// <summary>
            /// The phone number prefix
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("phone_number_prefix")]
            public string PhoneNumberPrefix { get; set; }
        }

        /// <summary>
        /// Returns information about a phone number by its prefix synchronously. getCountries must be called at least once after changing localization to the specified language if properly localized country information is expected. Can be called synchronously
        /// </summary>
        public static Task<PhoneNumberInfo> GetPhoneNumberInfoSyncAsync(
            this Client client, string languageCode = default, string phoneNumberPrefix = default)
        {
            return client.ExecuteAsync(new GetPhoneNumberInfoSync
            {
                LanguageCode = languageCode, PhoneNumberPrefix = phoneNumberPrefix
            });
        }
    }
}
// REUSE-IgnoreEnd