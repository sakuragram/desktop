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
        /// Returns a list of channel chats recommended to the current user
        /// </summary>
        public class GetRecommendedChats : Function<Chats>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getRecommendedChats";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns a list of channel chats recommended to the current user
        /// </summary>
        public static Task<Chats> GetRecommendedChatsAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetRecommendedChats
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd