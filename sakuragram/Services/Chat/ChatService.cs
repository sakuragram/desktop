using System.Threading.Tasks;
using sakuragram.Views.Chats;
using TdLib;

namespace sakuragram.Services;

public class ChatService
{
    private static readonly TdClient _client = App._client;
    public Chat CurrentChat { get; set; }
    
    public long _openedChatId;
    
    public bool _isChatOpen;
    public bool _isMediaOpen;
    
    public async Task OpenChat()
    {
        await _client.ExecuteAsync(new TdApi.OpenChat { ChatId = _openedChatId });
        _isChatOpen = true;
        CurrentChat = new Chat();
    }
    
    public async Task CloseChat()
    {
        await _client.ExecuteAsync(new TdApi.CloseChat { ChatId = _openedChatId });
        _isChatOpen = false;
        _isMediaOpen = false;
    }
}