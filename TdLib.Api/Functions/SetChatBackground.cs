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
        /// Sets the background in a specific chat. Supported only in private and secret chats with non-deleted users, and in chats with sufficient boost level and can_change_info administrator right
        /// </summary>
        public class SetChatBackground : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "setChatBackground";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Chat identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }

            /// <summary>
            /// The input background to use; pass null to create a new filled or chat theme background
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("background")]
            public InputBackground Background { get; set; }

            /// <summary>
            /// Background type; pass null to use default background type for the chosen background; backgroundTypeChatTheme isn't supported for private and secret chats.
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("type")]
            public BackgroundType Type { get; set; }

            /// <summary>
            /// Dimming of the background in dark themes, as a percentage; 0-100. Applied only to Wallpaper and Fill types of background
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("dark_theme_dimming")]
            public int DarkThemeDimming { get; set; }

            /// <summary>
            /// Pass true to set background only for self; pass false to set background for all chat users. Always false for backgrounds set in boosted chats. Background can be set for both users only by Telegram Premium users and if set background isn't of the type inputBackgroundPrevious
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("only_for_self")]
            public bool OnlyForSelf { get; set; }
        }

        /// <summary>
        /// Sets the background in a specific chat. Supported only in private and secret chats with non-deleted users, and in chats with sufficient boost level and can_change_info administrator right
        /// </summary>
        public static Task<Ok> SetChatBackgroundAsync(
            this Client client, long chatId = default, InputBackground background = default, BackgroundType type = default, int darkThemeDimming = default, bool onlyForSelf = default)
        {
            return client.ExecuteAsync(new SetChatBackground
            {
                ChatId = chatId, Background = background, Type = type, DarkThemeDimming = darkThemeDimming, OnlyForSelf = onlyForSelf
            });
        }
    }
}
// REUSE-IgnoreEnd