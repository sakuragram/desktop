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
        /// Sets default administrator rights for adding the bot to basic group and supergroup chats; for bots only
        /// </summary>
        public class SetDefaultGroupAdministratorRights : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "setDefaultGroupAdministratorRights";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Default administrator rights for adding the bot to basic group and supergroup chats; pass null to remove default rights
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("default_group_administrator_rights")]
            public ChatAdministratorRights DefaultGroupAdministratorRights { get; set; }
        }

        /// <summary>
        /// Sets default administrator rights for adding the bot to basic group and supergroup chats; for bots only
        /// </summary>
        public static Task<Ok> SetDefaultGroupAdministratorRightsAsync(
            this Client client, ChatAdministratorRights defaultGroupAdministratorRights = default)
        {
            return client.ExecuteAsync(new SetDefaultGroupAdministratorRights
            {
                DefaultGroupAdministratorRights = defaultGroupAdministratorRights
            });
        }
    }
}
// REUSE-IgnoreEnd