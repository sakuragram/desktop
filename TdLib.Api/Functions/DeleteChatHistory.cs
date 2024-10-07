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
        /// Deletes all messages in the chat. Use chat.can_be_deleted_only_for_self and chat.can_be_deleted_for_all_users fields to find whether and how the method can be applied to the chat
        /// </summary>
        public class DeleteChatHistory : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "deleteChatHistory";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Chat identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("chat_id")]
            public long ChatId { get; set; }

            /// <summary>
            /// Pass true to remove the chat from all chat lists
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("remove_from_chat_list")]
            public bool RemoveFromChatList { get; set; }

            /// <summary>
            /// Pass true to delete chat history for all users
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("revoke")]
            public bool Revoke { get; set; }
        }

        /// <summary>
        /// Deletes all messages in the chat. Use chat.can_be_deleted_only_for_self and chat.can_be_deleted_for_all_users fields to find whether and how the method can be applied to the chat
        /// </summary>
        public static Task<Ok> DeleteChatHistoryAsync(
            this Client client, long chatId = default, bool removeFromChatList = default, bool revoke = default)
        {
            return client.ExecuteAsync(new DeleteChatHistory
            {
                ChatId = chatId, RemoveFromChatList = removeFromChatList, Revoke = revoke
            });
        }
    }
}
// REUSE-IgnoreEnd