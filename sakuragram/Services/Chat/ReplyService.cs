using TdLib;

namespace sakuragram.Services;

public class ReplyService
{
    private long _chatId;
    private long _messageId;
    private TdClient _client = App._client;
    
    public void ReplyOnMessage(long chatId, long messageId, string messageText)
    {
        _chatId = chatId;
        _messageId = messageId;

        _client.ExecuteAsync(new TdApi.SendMessage
        {
            ChatId = _chatId,
            ReplyTo = new TdApi.InputMessageReplyTo.InputMessageReplyToMessage
            {
                MessageId = _messageId
            },
            InputMessageContent = new TdApi.InputMessageContent.InputMessageText
            {
                Text = new TdApi.FormattedText
                {
                    Text = messageText
                }
            }
        });
    }

    public long GetReplyMessageId()
    {
        return _messageId;
    }
    
    public void SelectMessageForReply(long messageId)
    {
        _messageId = messageId;
    }
}