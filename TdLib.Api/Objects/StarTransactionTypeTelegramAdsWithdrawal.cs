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
            /// The transaction is a withdrawal of earned Telegram Stars to Telegram Ad platform; for bots and channel chats only
            /// </summary>
            public class StarTransactionTypeTelegramAdsWithdrawal : StarTransactionType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "starTransactionTypeTelegramAdsWithdrawal";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }


            }
        }
    }
}
// REUSE-IgnoreEnd