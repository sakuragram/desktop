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
        /// Changes the photo of a chat. Supported only for basic groups, supergroups and channels. Requires can_change_info member right
        /// </summary>
        public class SetChatPhoto : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "setChatPhoto";

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
            /// New chat photo; pass null to delete the chat photo
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("photo")]
            public InputChatPhoto Photo { get; set; }
        }

        /// <summary>
        /// Changes the photo of a chat. Supported only for basic groups, supergroups and channels. Requires can_change_info member right
        /// </summary>
        public static Task<Ok> SetChatPhotoAsync(
            this Client client, long chatId = default, InputChatPhoto photo = default)
        {
            return client.ExecuteAsync(new SetChatPhoto
            {
                ChatId = chatId, Photo = photo
            });
        }
    }
}
// REUSE-IgnoreEnd