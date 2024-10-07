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
        /// Toggles whether a group call participant hand is rased
        /// </summary>
        public class ToggleGroupCallParticipantIsHandRaised : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "toggleGroupCallParticipantIsHandRaised";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Group call identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("group_call_id")]
            public int GroupCallId { get; set; }

            /// <summary>
            /// Participant identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("participant_id")]
            public MessageSender ParticipantId { get; set; }

            /// <summary>
            /// Pass true if the user's hand needs to be raised. Only self hand can be raised. Requires groupCall.can_be_managed group call flag to lower other's hand
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("is_hand_raised")]
            public bool IsHandRaised { get; set; }
        }

        /// <summary>
        /// Toggles whether a group call participant hand is rased
        /// </summary>
        public static Task<Ok> ToggleGroupCallParticipantIsHandRaisedAsync(
            this Client client, int groupCallId = default, MessageSender participantId = default, bool isHandRaised = default)
        {
            return client.ExecuteAsync(new ToggleGroupCallParticipantIsHandRaised
            {
                GroupCallId = groupCallId, ParticipantId = participantId, IsHandRaised = isHandRaised
            });
        }
    }
}
// REUSE-IgnoreEnd