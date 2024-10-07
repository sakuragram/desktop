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
        public partial class InternalLinkType : Object
        {
            /// <summary>
            /// The link is an invite link to a chat folder. Call checkChatFolderInviteLink with the given invite link to process the link.
            /// If the link is valid and the user wants to join the chat folder, then call addChatFolderByInviteLink
            /// </summary>
            public class InternalLinkTypeChatFolderInvite : InternalLinkType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "internalLinkTypeChatFolderInvite";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Internal representation of the invite link
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("invite_link")]
                public string InviteLink { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd