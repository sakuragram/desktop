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
        /// <summary>
        /// Contains information about interactions with a story
        /// </summary>
        public partial class StoryInteractionInfo : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "storyInteractionInfo";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Number of times the story was viewed
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("view_count")]
            public int ViewCount { get; set; }

            /// <summary>
            /// Number of times the story was forwarded; 0 if none or unknown
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("forward_count")]
            public int ForwardCount { get; set; }

            /// <summary>
            /// Number of reactions added to the story; 0 if none or unknown
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("reaction_count")]
            public int ReactionCount { get; set; }

            /// <summary>
            /// Identifiers of at most 3 recent viewers of the story
            /// </summary>
            [JsonProperty("recent_viewer_user_ids", ItemConverterType = typeof(Converter))]
            public long[] RecentViewerUserIds { get; set; }
        }
    }
}
// REUSE-IgnoreEnd