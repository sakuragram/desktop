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
        /// Clears the list of recently found chats
        /// </summary>
        public class ClearRecentlyFoundChats : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "clearRecentlyFoundChats";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Clears the list of recently found chats
        /// </summary>
        public static Task<Ok> ClearRecentlyFoundChatsAsync(
            this Client client)
        {
            return client.ExecuteAsync(new ClearRecentlyFoundChats
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd