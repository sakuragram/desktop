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
        public partial class AffiliateProgramSortOrder : Object
        {
            /// <summary>
            /// The affiliate programs must be sorted by the expected revenue
            /// </summary>
            public class AffiliateProgramSortOrderRevenue : AffiliateProgramSortOrder
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "affiliateProgramSortOrderRevenue";

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