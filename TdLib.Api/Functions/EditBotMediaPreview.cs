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
        /// Replaces media preview in the list of media previews of a bot. Returns the new preview after edit is completed server-side
        /// </summary>
        public class EditBotMediaPreview : Function<BotMediaPreview>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "editBotMediaPreview";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the target bot. The bot must be owned and must have the main Web App
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("bot_user_id")]
            public long BotUserId { get; set; }

            /// <summary>
            /// Language code of the media preview to edit
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("language_code")]
            public string LanguageCode { get; set; }

            /// <summary>
            /// File identifier of the media to replace
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("file_id")]
            public int FileId { get; set; }

            /// <summary>
            /// Content of the new preview
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("content")]
            public InputStoryContent Content { get; set; }
        }

        /// <summary>
        /// Replaces media preview in the list of media previews of a bot. Returns the new preview after edit is completed server-side
        /// </summary>
        public static Task<BotMediaPreview> EditBotMediaPreviewAsync(
            this Client client, long botUserId = default, string languageCode = default, int fileId = default, InputStoryContent content = default)
        {
            return client.ExecuteAsync(new EditBotMediaPreview
            {
                BotUserId = botUserId, LanguageCode = languageCode, FileId = fileId, Content = content
            });
        }
    }
}
// REUSE-IgnoreEnd