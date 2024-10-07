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
        public partial class Update : Object
        {
            /// <summary>
            /// The Telegram Star revenue earned by a bot or a chat has changed. If Telegram Star transaction screen of the chat is opened, then getStarTransactions may be called to fetch new transactions
            /// </summary>
            public class UpdateStarRevenueStatus : Update
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "updateStarRevenueStatus";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Identifier of the owner of the Telegram Stars
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("owner_id")]
                public MessageSender OwnerId { get; set; }

                /// <summary>
                /// New Telegram Star revenue status
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("status")]
                public StarRevenueStatus Status { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd