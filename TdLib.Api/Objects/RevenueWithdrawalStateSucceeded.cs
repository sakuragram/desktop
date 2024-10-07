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
        public partial class RevenueWithdrawalState : Object
        {
            /// <summary>
            /// Withdrawal succeeded
            /// </summary>
            public class RevenueWithdrawalStateSucceeded : RevenueWithdrawalState
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "revenueWithdrawalStateSucceeded";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Point in time (Unix timestamp) when the withdrawal was completed
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("date")]
                public int Date { get; set; }

                /// <summary>
                /// The URL where the withdrawal transaction can be viewed
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("url")]
                public string Url { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd