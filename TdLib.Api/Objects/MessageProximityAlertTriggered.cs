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
            /// A user in the chat came within proximity alert range
            /// </summary>
            public class MessageProximityAlertTriggered : MessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messageProximityAlertTriggered";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The identifier of a user or chat that triggered the proximity alert
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("traveler_id")]
                public MessageSender TravelerId { get; set; }

                /// <summary>
                /// The identifier of a user or chat that subscribed for the proximity alert
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("watcher_id")]
                public MessageSender WatcherId { get; set; }

                /// <summary>
                /// The distance between the users
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("distance")]
                public int Distance { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd