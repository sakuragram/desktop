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
        /// Returns the list of commands supported by the bot for the given user scope and language; for bots only
        /// </summary>
        public class GetCommands : Function<BotCommands>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getCommands";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The scope to which the commands are relevant; pass null to get commands in the default bot command scope
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("scope")]
            public BotCommandScope Scope { get; set; }

            /// <summary>
            /// A two-letter ISO 639-1 language code or an empty string
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("language_code")]
            public string LanguageCode { get; set; }
        }

        /// <summary>
        /// Returns the list of commands supported by the bot for the given user scope and language; for bots only
        /// </summary>
        public static Task<BotCommands> GetCommandsAsync(
            this Client client, BotCommandScope scope = default, string languageCode = default)
        {
            return client.ExecuteAsync(new GetCommands
            {
                Scope = scope, LanguageCode = languageCode
            });
        }
    }
}
// REUSE-IgnoreEnd