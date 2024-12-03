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

public partial class MessageThreadList
{
    private TdClient _client = App._client;
    private MessageService _messageService;
    private ReplyService _replyService;
    private Chat _openedChat;
    private ChatMessage _lastChatMessage;
    private long _chatId;
    
/// <summary>
/// Initializes a new instance of the <see cref="MessageThreadList"/> class.
/// </summary>
/// <remarks>
/// This constructor sets up the components required for displaying a list of message threads
/// within the chat list view.
/// </remarks>
    public MessageThreadList() => InitializeComponent();

    public async Task PrepareMessageThread(long chatId, long messageThreadId, Chat openedChat, MessageService messageService, ReplyService replyService)
    {
        _openedChat = openedChat;
        _messageService = messageService;
        _replyService = replyService;
        _chatId = chatId;
        
        await DispatcherQueue.EnqueueAsync(async () =>
        {
            var messages = await _client.ExecuteAsync(new TdApi.GetMessageThreadHistory
            {
                ChatId = chatId,
                MessageId = messageThreadId,
                Limit = 42
            });
            
            await GenerateMessageByType(messages.Messages_.Reverse().ToList(), true);
        });
    }

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
                    var lastMessage = await _client.GetMessageAsync(_chatId, _lastChatMessage._messageId);
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