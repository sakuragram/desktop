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
        public partial class CanTransferOwnershipResult : Object
        {
            /// <summary>
            /// Represents result of checking whether the current session can be used to transfer a chat ownership to another user
            /// </summary>
            public class CanTransferOwnershipResultOk : CanTransferOwnershipResult
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "canTransferOwnershipResultOk";

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