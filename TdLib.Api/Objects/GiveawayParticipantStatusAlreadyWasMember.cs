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
        public partial class GiveawayParticipantStatus : Object
        {
            /// <summary>
            /// The user can't participate in the giveaway, because they have already been member of the chat
            /// </summary>
            public class GiveawayParticipantStatusAlreadyWasMember : GiveawayParticipantStatus
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "giveawayParticipantStatusAlreadyWasMember";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Point in time (Unix timestamp) when the user joined the chat
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("joined_chat_date")]
                public int JoinedChatDate { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd