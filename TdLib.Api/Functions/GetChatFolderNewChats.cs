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
        /// Returns new chats added to a shareable chat folder by its owner. The method must be called at most once in getOption("chat_folder_new_chats_update_period") for the given chat folder
        /// </summary>
        public class GetChatFolderNewChats : Function<Chats>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "getChatFolderNewChats";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Chat folder identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_folder_id")]
            public int ChatFolderId { get; set; }
        }

        /// <summary>
        /// Returns new chats added to a shareable chat folder by its owner. The method must be called at most once in getOption("chat_folder_new_chats_update_period") for the given chat folder
        /// </summary>
        public static Task<Chats> GetChatFolderNewChatsAsync(
            this Client client, int chatFolderId = default)
        {
            return client.ExecuteAsync(new GetChatFolderNewChats
            {
                ChatFolderId = chatFolderId
            });
        }
    }
}
// REUSE-IgnoreEnd