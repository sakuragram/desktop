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
        /// Returns auto-download settings presets for the current user
        /// </summary>
        public class GetAutoDownloadSettingsPresets : Function<AutoDownloadSettingsPresets>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getAutoDownloadSettingsPresets";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }


        }

        /// <summary>
        /// Returns auto-download settings presets for the current user
        /// </summary>
        public static Task<AutoDownloadSettingsPresets> GetAutoDownloadSettingsPresetsAsync(
            this Client client)
        {
            return client.ExecuteAsync(new GetAutoDownloadSettingsPresets
            {
                
            });
        }
    }
}
// REUSE-IgnoreEnd