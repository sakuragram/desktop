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
        /// Returns greeting stickers from regular sticker sets that can be used for the start page of other users
        /// </summary>
        public class GetGreetingStickers : Function<Stickers>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getGreetingStickers";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns greeting stickers from regular sticker sets that can be used for the start page of other users
        /// </summary>
        public static Task<Stickers> GetGreetingStickersAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetGreetingStickers
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd