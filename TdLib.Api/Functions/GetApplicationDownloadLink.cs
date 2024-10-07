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
        /// Returns the link for downloading official Telegram application to be used when the current user invites friends to Telegram
        /// </summary>
        public class GetApplicationDownloadLink : Function<HttpUrl>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getApplicationDownloadLink";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns the link for downloading official Telegram application to be used when the current user invites friends to Telegram
        /// </summary>
        public static Task<HttpUrl> GetApplicationDownloadLinkAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetApplicationDownloadLink
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd