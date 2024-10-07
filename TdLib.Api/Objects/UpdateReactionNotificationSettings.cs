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
            /// Notification settings for reactions were updated
            /// </summary>
            public class UpdateReactionNotificationSettings : Update
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "updateReactionNotificationSettings";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// The new notification settings
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("notification_settings")]
                public ReactionNotificationSettings NotificationSettings { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd