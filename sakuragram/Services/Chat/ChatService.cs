using System;
using System.Diagnostics;
using System.Threading.Tasks;
using sakuragram.Views.Chats;
using TdLib;

namespace sakuragram.Services;

public class ChatService
{
    private static readonly TdClient _client = App._client;
    
    public Views.Chats.Chat CurrentChat { get; set; }
    
    public long _openedChatId;
    
    public bool _isChatOpen;
    public bool _isMediaOpen;
    
    public async Task<Views.Chats.Chat> OpenChat(long chatId)
    {
        try
        {
            _openedChatId = chatId;
            _isChatOpen = true;
            CurrentChat = new Views.Chats.Chat(_openedChatId, false, null);
            return CurrentChat;
        }
        catch (TdException e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }

    public async Task<Views.Chats.Chat> OpenForum(long chatId, TdApi.ForumTopic forumTopic)
    {
        try
        {
            _openedChatId = chatId;
            _isChatOpen = true;
            CurrentChat = new Views.Chats.Chat(_openedChatId, true, forumTopic);
            return CurrentChat;
        }
        catch (TdException e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }
    
    public async Task CloseChat()
    {
        await _client.ExecuteAsync(new TdApi.CloseChat { ChatId = _openedChatId });
        _isChatOpen = false;
        _isMediaOpen = false;
    }
}