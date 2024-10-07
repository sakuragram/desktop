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
        public partial class ChatEventAction : Object
        {
            /// <summary>
            /// The chat's profile accent color or profile background custom emoji were changed
            /// </summary>
            public class ChatEventProfileAccentColorChanged : ChatEventAction
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "chatEventProfileAccentColorChanged";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Previous identifier of chat's profile accent color; -1 if none
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("old_profile_accent_color_id")]
                public int OldProfileAccentColorId { get; set; }

                /// <summary>
                /// Previous identifier of the custom emoji; 0 if none
                /// </summary>
                [JsonConverter(typeof(Converter.Int64))]
                [JsonProperty("old_profile_background_custom_emoji_id")]
                public long OldProfileBackgroundCustomEmojiId { get; set; }

                /// <summary>
                /// New identifier of chat's profile accent color; -1 if none
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("new_profile_accent_color_id")]
                public int NewProfileAccentColorId { get; set; }

                /// <summary>
                /// New identifier of the custom emoji; 0 if none
                /// </summary>
                [JsonConverter(typeof(Converter.Int64))]
                [JsonProperty("new_profile_background_custom_emoji_id")]
                public long NewProfileBackgroundCustomEmojiId { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd