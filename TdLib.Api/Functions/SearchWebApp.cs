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
        /// Returns information about a Web App by its short name. Returns a 404 error if the Web App is not found
        /// </summary>
        public class SearchWebApp : Function<FoundWebApp>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "searchWebApp";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Identifier of the target bot
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("bot_user_id")]
            public long BotUserId { get; set; }

            /// <summary>
            /// Short name of the Web App
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("web_app_short_name")]
            public string WebAppShortName { get; set; }
        }

        /// <summary>
        /// Returns information about a Web App by its short name. Returns a 404 error if the Web App is not found
        /// </summary>
        public static Task<FoundWebApp> SearchWebAppAsync(
            this Client client, long botUserId = default, string webAppShortName = default)
        {
            return client.ExecuteAsync(new SearchWebApp
            {
                BotUserId = botUserId, WebAppShortName = webAppShortName
            });
        }
    }
}
// REUSE-IgnoreEnd