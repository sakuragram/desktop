using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using sakuragram.Views.Profile;
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

    public static async Task<SenderType> GetSender(TdApi.MessageSender sender)
    {
        if (sender == null) return new SenderType(null, null);
        switch (sender)
        {
            case TdApi.MessageSender.MessageSenderUser u:
                try
                {
                    TdApi.User foundedUser = await _client.GetUserAsync(u.UserId);
                    return new SenderType(foundedUser, null);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return new SenderType(null, null);
                }
            
            case TdApi.MessageSender.MessageSenderChat c:
                try
                {
                    TdApi.Chat foundedChat = await _client.GetChatAsync(c.ChatId);
                    return new SenderType(null, foundedChat);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return new SenderType(null, null);
                }
            
            default:
                return new SenderType(null, null);
        }
    }

    public static async Task<string> GetSenderName(TdApi.Message message, bool needForwardInfo = false)
    {
        if (message == null) return null;
        try
        {
            var sender = await GetSender(message.SenderId);
            TdApi.Chat chat = await _client.GetChatAsync(message.ChatId);

            if (needForwardInfo)
            {
                if (message.ForwardInfo != null)
                {
                    if (message.ForwardInfo.Source != null)
                    {
                        if (message.ForwardInfo.Source.SenderName != string.Empty)
                            return message.ForwardInfo.Source.SenderName;
                        else
                        {
                            var user = GetSender(message.ForwardInfo.Source.SenderId).Result;
                            if (user.User != null)
                                return user.User.LastName != string.Empty
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
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
        
        return null;
    }

    public static string GetUserStatus(TdApi.UserStatus status)
    {
        return status switch
        {
            TdApi.UserStatus.UserStatusEmpty => "empty",
            TdApi.UserStatus.UserStatusLastMonth => "last month",
            TdApi.UserStatus.UserStatusLastWeek => "last week",
            TdApi.UserStatus.UserStatusOffline => "offline",
            TdApi.UserStatus.UserStatusOnline => "online",
            TdApi.UserStatus.UserStatusRecently => "last seen recently",
            _ => "status not found"
        };
    }
    
    public static async Task ShowProfile(TdApi.User user = null, TdApi.Chat chat = null, XamlRoot xamlRoot = null)
    {
        if (xamlRoot == null) return;
        Profile profile = new() { XamlRoot = xamlRoot, };
        await profile.PrepareProfileWindow(user, chat);
        await profile.ShowAsync();
    }
}