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
        public partial class StarTransactionType : Object
        {
            /// <summary>
            /// The transaction is a purchase of a gift to another user; for regular users and bots only
            /// </summary>
            public class StarTransactionTypeGiftPurchase : StarTransactionType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "starTransactionTypeGiftPurchase";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Identifier of the user that received the gift
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("user_id")]
                public long UserId { get; set; }

                /// <summary>
                /// The gift
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("gift")]
                public Gift Gift { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd