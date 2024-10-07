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
        /// Returns interactions with a story posted in a chat. Can be used only if story is posted on behalf of a chat and the user is an administrator in the chat
        /// </summary>
        public class GetChatStoryInteractions : Function<StoryInteractions>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getChatStoryInteractions";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// The identifier of the sender of the story
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("story_sender_chat_id")]
            public long StorySenderChatId { get; set; }

            /// <summary>
            /// Story identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("story_id")]
            public int StoryId { get; set; }

            /// <summary>
            /// Pass the default heart reaction or a suggested reaction type to receive only interactions with the specified reaction type; pass null to receive all interactions; reactionTypePaid isn't supported
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("reaction_type")]
            public ReactionType ReactionType { get; set; }

            /// <summary>
            /// Pass true to get forwards and reposts first, then reactions, then other views; pass false to get interactions sorted just by interaction date
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("prefer_forwards")]
            public bool PreferForwards { get; set; }

            /// <summary>
            /// Offset of the first entry to return as received from the previous request; use empty string to get the first chunk of results
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("offset")]
            public string Offset { get; set; }

            /// <summary>
            /// The maximum number of story interactions to return
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("limit")]
            public int Limit { get; set; }
        }

        /// <summary>
        /// Returns interactions with a story posted in a chat. Can be used only if story is posted on behalf of a chat and the user is an administrator in the chat
        /// </summary>
        public static Task<StoryInteractions> GetChatStoryInteractionsAsync(
            this Client client, long storySenderChatId = default, int storyId = default, ReactionType reactionType = default, bool preferForwards = default, string offset = default, int limit = default)
        {
            return client.ExecuteAsync(new GetChatStoryInteractions
            {
                StorySenderChatId = storySenderChatId, StoryId = storyId, ReactionType = reactionType, PreferForwards = preferForwards, Offset = offset, Limit = limit
            });
        }
    }
}
// REUSE-IgnoreEnd