using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using sakuragram.Services;
using sakuragram.Services.Chat;
using sakuragram.Services.Core;
using sakuragram.Views.Chats;
using sakuragram.Views.Chats.Messages;
using TdLib;

namespace sakuragram.Views.ChatList;

public partial class ChatHistoryList
{
    private TdClient _client = App._client;
    private TdApi.Chat _chat;
    private MessageService _messageService;
    private ReplyService _replyService;
    private Chat _openedChat;
    private ChatMessage _lastChatMessage;
    
    /// <summary>
    /// A list of chat entries that represents a chat history.
    /// </summary>
    public ChatHistoryList() => InitializeComponent();

    public async Task PrepareChatList(Chat openedChat, TdApi.Chat chat, MessageService messageService, ReplyService replyService)
    {
        _openedChat = openedChat;
        _chat = chat;
        _messageService = messageService;
        _replyService = replyService;
        long fromMessageId = 0;
        
        await DispatcherQueue.EnqueueAsync(async () =>
        {
            if (chat.LastReadInboxMessageId == chat.LastMessage.Id)
                fromMessageId = chat.LastReadInboxMessageId;
            else if (chat.LastReadOutboxMessageId == chat.LastMessage.Id)
                fromMessageId = chat.LastReadOutboxMessageId;
            
            var messages = await _client.ExecuteAsync(new TdApi.GetChatHistory
            {
                ChatId = chat.Id,
                FromMessageId = fromMessageId,
                Limit = 42,
                OnlyLocal = false
            });
            
            await GenerateMessageByType(messages.Messages_.Reverse().ToList(), true);
        });
    }
    
    /// <summary>
    /// Generates and displays chat messages in the UI based on the provided list of messages.
    /// </summary>
    /// <param name="messages">A list of TdApi.Message objects to be displayed.</param>
    /// <param name="clear">A boolean indicating whether to clear the existing messages before adding new ones.</param>
    /// <returns>An asynchronous task representing the operation.</returns>
    private async Task GenerateMessageByType(List<TdApi.Message> messages, bool clear = false)
    {
        List<TdApi.Message> addedMessages = [];
        
        await DispatcherQueue.EnqueueAsync(async () =>
        {
            if (clear) MessagesList.Children.Clear();
            
            foreach (var message in messages.Where(message => !addedMessages.Contains(message)))
            {
                if (_lastChatMessage != null)
                {
                    var lastMessage = await _client.GetMessageAsync(_chat.Id, _lastChatMessage._messageId);
                    var lastMessageReadTime = MathService.CalculateDateTime(lastMessage.Date);
                    var messageReadTime = MathService.CalculateDateTime(message.Date);
                    
                    if (messageReadTime.Day > lastMessageReadTime.Day)
                    {
                        var serviceMessage = new ChatServiceMessage();
                        serviceMessage.UpdateDateMessage(messageReadTime, lastMessageReadTime);
                        MessagesList.Children.Add(serviceMessage);
                    }
                    
                    if (message.MediaAlbumId != 0 && _lastChatMessage._mediaAlbumId == message.MediaAlbumId) 
                    {
                        _lastChatMessage.AddAlbumElement(message);
                        addedMessages.Add(message);
                        continue;
                    }
                }
                
                var messageEntry = new ChatMessage(_openedChat, _replyService);
                _lastChatMessage = messageEntry;
                messageEntry._messageService = _messageService;
                await messageEntry.UpdateMessage(message);
                MessagesList.Children.Add(messageEntry);
                addedMessages.Add(message);
            }
        });
    }
}