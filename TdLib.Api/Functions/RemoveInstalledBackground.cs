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
        /// Removes background from the list of installed backgrounds
        /// </summary>
        public class RemoveInstalledBackground : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "removeInstalledBackground";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The background identifier
            /// </summary>
            [JsonConverter(typeof(Converter.Int64))]
            [JsonProperty("background_id")]
            public long BackgroundId { get; set; }
        }

        /// <summary>
        /// Removes background from the list of installed backgrounds
        /// </summary>
        public static Task<Ok> RemoveInstalledBackgroundAsync(
            this Client client, long backgroundId = default)
        {
            return client.ExecuteAsync(new RemoveInstalledBackground
            {
                BackgroundId = backgroundId
            });
        }
    }
}
// REUSE-IgnoreEnd