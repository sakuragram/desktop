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
        public partial class StoryAreaType : Object
        {
            /// <summary>
            /// An area pointing to a venue
            /// </summary>
            public class StoryAreaTypeVenue : StoryAreaType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "storyAreaTypeVenue";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Information about the venue
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("venue")]
                public Venue Venue { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd