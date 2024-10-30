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
        
        if (message.ForwardInfo != null)
        {
            if (message.ForwardInfo.Source != null)
            {
                if (message.ForwardInfo.Source.SenderName != string.Empty) 
                    return Task.FromResult(message.ForwardInfo.Source.SenderName);
                else
                {
                    var user = GetSender(message.ForwardInfo.Source.SenderId).Result; 
                    if (user.User != null) return Task.FromResult(user.User.FirstName + " " + user.User.LastName);
                    if (user.Chat != null) return Task.FromResult(user.Chat.Title);
                }
            }
            else if (message.ForwardInfo.Origin != null)
            {
                switch (message.ForwardInfo.Origin)
                {
                    case TdApi.MessageOrigin.MessageOriginChannel originChannel:
                    {
                        var foundedChannel = _client.GetChatAsync(chatId: originChannel.ChatId).Result;
                        string forwardInfo = foundedChannel.Title;

                        if (originChannel.AuthorSignature != string.Empty)
                        {
                            forwardInfo = forwardInfo + $" ({originChannel.AuthorSignature})";
                        }
                        
                        return Task.FromResult(forwardInfo);
                    }
                    case TdApi.MessageOrigin.MessageOriginChat originChat:
                    {
                        return Task.FromResult(originChat.AuthorSignature);
                    }
                    case TdApi.MessageOrigin.MessageOriginUser user:
                    {
                        var originUser = _client.GetUserAsync(userId: user.SenderUserId).Result;
                        return Task.FromResult(originUser.FirstName + " " + originUser.LastName);
                    }
                    case TdApi.MessageOrigin.MessageOriginHiddenUser hiddenUser:
                    {
                        return Task.FromResult(hiddenUser.SenderName);
                    }
                }
            }
        }
        
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