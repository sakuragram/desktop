using System;
using System.Threading.Tasks;
using TdLib;

namespace sakuragram.Services;

public class UserService
{
    private static TdClient _client = App._client;
    
    public struct SenderType(TdApi.User user, TdApi.Chat chat)
    {
        public TdApi.User User { get; set; } = user;
        public TdApi.Chat Chat { get; set; } = chat;
    }

    public static Task<SenderType> GetSender(TdApi.MessageSender sender)
    {
        switch (sender)
        {
            case TdApi.MessageSender.MessageSenderUser u:
                TdApi.User foundedUser = _client.GetUserAsync(u.UserId).Result;
                return Task.FromResult(new SenderType(foundedUser, null));
            
            case TdApi.MessageSender.MessageSenderChat c:
                TdApi.Chat foundedChat = _client.GetChatAsync(c.ChatId).Result;
                return Task.FromResult(new SenderType(null, foundedChat));
            
            default:
                return Task.FromResult(new SenderType(null, null));
        }
    }

    public static Task<string> GetSenderName(TdApi.Message message)
    {
        var sender = GetSender(message.SenderId).Result;
        TdApi.Chat chat = _client.GetChatAsync(message.ChatId).Result;
        
        if (sender.User != null)
        {
            return chat.Type switch
            {
                TdApi.ChatType.ChatTypePrivate or TdApi.ChatType.ChatTypeSecret => Task.FromResult(string.Empty),
                _ => Task.FromResult(sender.User.FirstName + " " + sender.User.LastName)
            };
        }
        else if (sender.Chat != null)
        {
            return chat.Type switch
            {
                TdApi.ChatType.ChatTypeSupergroup typeSupergroup => Task.FromResult(typeSupergroup.IsChannel
                    ? string.Empty
                    : sender.Chat.Title),
                TdApi.ChatType.ChatTypePrivate or TdApi.ChatType.ChatTypeSecret => Task.FromResult(string.Empty),
                _ => Task.FromResult(sender.Chat.Title)
            };
        }

        return null;
    }
}