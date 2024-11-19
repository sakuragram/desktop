using TdLib;

namespace sakuragram.Services.Chat;

public class ReplyService
{
    private long _chatId;
    private long _messageId;
    private TdClient _client = App._client;
    
    public void ReplyOnMessage(long chatId, string messageText)
    {
        _chatId = chatId;

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