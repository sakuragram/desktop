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

    public static async Task<string> GetSenderName(TdApi.Message message)
    {
        var sender = await GetSender(message.SenderId);
        TdApi.Chat chat = await _client.GetChatAsync(message.ChatId);
        
        if (message.ForwardInfo != null)
        {
            if (message.ForwardInfo.Source != null)
            {
                if (message.ForwardInfo.Source.SenderName != string.Empty) 
                    return message.ForwardInfo.Source.SenderName;
                else
                {
                    var user = GetSender(message.ForwardInfo.Source.SenderId).Result; 
                    if (user.User != null) return user.User.LastName != string.Empty 
                        ? user.User.FirstName + " " + user.User.LastName
                        : user.User.FirstName;
                    if (user.Chat != null) return user.Chat.Title;
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
                        
                        return forwardInfo;
                    }
                    case TdApi.MessageOrigin.MessageOriginChat originChat:
                    {
                        return originChat.AuthorSignature;
                    }
                    case TdApi.MessageOrigin.MessageOriginUser user:
                    {
                        var originUser = _client.GetUserAsync(userId: user.SenderUserId).Result;
                        return originUser.FirstName + " " + originUser.LastName;
                    }
                    case TdApi.MessageOrigin.MessageOriginHiddenUser hiddenUser:
                    {
                        return hiddenUser.SenderName;
                    }
                }
            }
        }
        
        if (sender.User != null)
        {
            var currentUser = await _client.GetMeAsync();
            
            return chat.Type switch
            {
                TdApi.ChatType.ChatTypePrivate or TdApi.ChatType.ChatTypeSecret => string.Empty,
                _ => sender.User.Id != currentUser.Id
                ? sender.User.LastName != string.Empty 
                    ? sender.User.FirstName + " " + sender.User.LastName 
                    : sender.User.FirstName
                : "You"
            };
        }
        if (sender.Chat != null)
        {
            return chat.Type switch
            {
                TdApi.ChatType.ChatTypeSupergroup typeSupergroup => typeSupergroup.IsChannel
                    ? string.Empty
                    : sender.Chat.Title,
                TdApi.ChatType.ChatTypePrivate or TdApi.ChatType.ChatTypeSecret => string.Empty,
                _ => sender.Chat.Title
            };
        }
        
        return null;
    }
}