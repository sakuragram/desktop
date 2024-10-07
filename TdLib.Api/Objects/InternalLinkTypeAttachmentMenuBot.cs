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
            /// The link is a link to an attachment menu bot to be opened in the specified or a chosen chat. Process given target_chat to open the chat.
            /// Then, call searchPublicChat with the given bot username, check that the user is a bot and can be added to attachment menu. Then, use getAttachmentMenuBot to receive information about the bot.
            /// If the bot isn't added to attachment menu, then show a disclaimer about Mini Apps being third-party applications, ask the user to accept their Terms of service and confirm adding the bot to side and attachment menu.
            /// If the user accept the terms and confirms adding, then use toggleBotIsAddedToAttachmentMenu to add the bot.
            /// If the attachment menu bot can't be used in the opened chat, show an error to the user. If the bot is added to attachment menu and can be used in the chat, then use openWebApp with the given URL
            /// </summary>
            public class InternalLinkTypeAttachmentMenuBot : InternalLinkType
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "internalLinkTypeAttachmentMenuBot";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Target chat to be opened
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("target_chat")]
                public TargetChat TargetChat { get; set; }

                /// <summary>
                /// Username of the bot
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("bot_username")]
                public string BotUsername { get; set; }

                /// <summary>
                /// URL to be passed to openWebApp
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("url")]
                public string Url { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd