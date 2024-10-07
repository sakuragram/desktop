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
        /// Changes the chat members permissions. Supported only for basic groups and supergroups. Requires can_restrict_members administrator right
        /// </summary>
        public class SetChatPermissions : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "setChatPermissions";

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
            /// New non-administrator members permissions in the chat
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("permissions")]
            public ChatPermissions Permissions { get; set; }
        }

        /// <summary>
        /// Changes the chat members permissions. Supported only for basic groups and supergroups. Requires can_restrict_members administrator right
        /// </summary>
        public static Task<Ok> SetChatPermissionsAsync(
            this Client client, long chatId = default, ChatPermissions permissions = default)
        {
            return client.ExecuteAsync(new SetChatPermissions
            {
                ChatId = chatId, Permissions = permissions
            });
        }
    }
}
// REUSE-IgnoreEnd