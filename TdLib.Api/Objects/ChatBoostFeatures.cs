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
        /// <summary>
        /// Contains a list of features available on the first chat boost levels
        /// </summary>
        public partial class ChatBoostFeatures : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "chatBoostFeatures";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The list of features
            /// </summary>
            [JsonProperty("features", ItemConverterType = typeof(Converter))]
            public ChatBoostLevelFeatures[] Features { get; set; }

            /// <summary>
            /// The minimum boost level required to set custom emoji for profile background
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_profile_background_custom_emoji_boost_level")]
            public int MinProfileBackgroundCustomEmojiBoostLevel { get; set; }

            /// <summary>
            /// The minimum boost level required to set custom emoji for reply header and link preview background; for channel chats only
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_background_custom_emoji_boost_level")]
            public int MinBackgroundCustomEmojiBoostLevel { get; set; }

            /// <summary>
            /// The minimum boost level required to set emoji status
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_emoji_status_boost_level")]
            public int MinEmojiStatusBoostLevel { get; set; }

            /// <summary>
            /// The minimum boost level required to set a chat theme background as chat background
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_chat_theme_background_boost_level")]
            public int MinChatThemeBackgroundBoostLevel { get; set; }

            /// <summary>
            /// The minimum boost level required to set custom chat background
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_custom_background_boost_level")]
            public int MinCustomBackgroundBoostLevel { get; set; }

            /// <summary>
            /// The minimum boost level required to set custom emoji sticker set for the chat; for supergroup chats only
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_custom_emoji_sticker_set_boost_level")]
            public int MinCustomEmojiStickerSetBoostLevel { get; set; }

            /// <summary>
            /// The minimum boost level allowing to recognize speech in video note and voice note messages for non-Premium users; for supergroup chats only
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_speech_recognition_boost_level")]
            public int MinSpeechRecognitionBoostLevel { get; set; }

            /// <summary>
            /// The minimum boost level allowing to disable sponsored messages in the chat; for channel chats only
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("min_sponsored_message_disable_boost_level")]
            public int MinSponsoredMessageDisableBoostLevel { get; set; }
        }
    }
}
// REUSE-IgnoreEnd