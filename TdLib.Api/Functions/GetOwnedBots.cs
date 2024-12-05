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
        /// Returns the list of owned by the current user bots
        /// </summary>
        public class GetOwnedBots : Function<Users>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getOwnedBots";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns the list of owned by the current user bots
        /// </summary>
        public static Task<Users> GetOwnedBotsAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetOwnedBots
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd