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
        /// Edits existing chat folder. Returns information about the edited chat folder
        /// </summary>
        public class EditChatFolder : Function<ChatFolderInfo>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "editChatFolder";

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

            /// <summary>
            /// The edited chat folder
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("folder")]
            public ChatFolder Folder { get; set; }
        }

        /// <summary>
        /// Edits existing chat folder. Returns information about the edited chat folder
        /// </summary>
        public static Task<ChatFolderInfo> EditChatFolderAsync(
            this Client client, int chatFolderId = default, ChatFolder folder = default)
        {
            return client.ExecuteAsync(new EditChatFolder
            {
                ChatFolderId = chatFolderId, Folder = folder
            });
        }
    }
}
// REUSE-IgnoreEnd