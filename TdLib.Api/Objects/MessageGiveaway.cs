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
        public partial class MessageContent : Object
        {
            /// <summary>
            /// A giveaway
            /// </summary>
            public class MessageGiveaway : MessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageGiveaway";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Giveaway parameters
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("parameters")]
                public GiveawayParameters Parameters { get; set; }

                /// <summary>
                /// Number of users which will receive Telegram Premium subscription gift codes
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("winner_count")]
                public int WinnerCount { get; set; }

                /// <summary>
                /// Prize of the giveaway
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("prize")]
                public GiveawayPrize Prize { get; set; }

                /// <summary>
                /// A sticker to be shown in the message; may be null if unknown
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("sticker")]
                public Sticker Sticker { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd