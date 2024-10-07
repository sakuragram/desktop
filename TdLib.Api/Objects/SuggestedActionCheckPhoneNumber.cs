using System;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        public partial class SuggestedAction : Object
        {
            /// <summary>
            /// Suggests the user to check whether authorization phone number is correct and change the phone number if it is inaccessible
            /// </summary>
            public class SuggestedActionCheckPhoneNumber : SuggestedAction
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "suggestedActionCheckPhoneNumber";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }


            }
        }
    }
}
// REUSE-IgnoreEnd