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
        public partial class SuggestedAction : Object
        {
            /// <summary>
            /// Suggests the user to extend their expiring Telegram Premium subscription
            /// </summary>
            public class SuggestedActionExtendPremium : SuggestedAction
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "suggestedActionExtendPremium";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// A URL for managing Telegram Premium subscription
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("manage_premium_subscription_url")]
                public string ManagePremiumSubscriptionUrl { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd