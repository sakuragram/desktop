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
        /// Contains settings for automatic moving of chats to and from the Archive chat lists
        /// </summary>
        public partial class ArchiveChatListSettings : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "archiveChatListSettings";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// True, if new chats from non-contacts will be automatically archived and muted. Can be set to true only if the option "can_archive_and_mute_new_chats_from_unknown_users" is true
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("archive_and_mute_new_chats_from_unknown_users")]
            public bool ArchiveAndMuteNewChatsFromUnknownUsers { get; set; }

            /// <summary>
            /// True, if unmuted chats will be kept in the Archive chat list when they get a new message
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("keep_unmuted_chats_archived")]
            public bool KeepUnmutedChatsArchived { get; set; }

            /// <summary>
            /// True, if unmuted chats, that are always included or pinned in a folder, will be kept in the Archive chat list when they get a new message. Ignored if keep_unmuted_chats_archived == true
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("keep_chats_from_folders_archived")]
            public bool KeepChatsFromFoldersArchived { get; set; }
        }
    }
}
// REUSE-IgnoreEnd