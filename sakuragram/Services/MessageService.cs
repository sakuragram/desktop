using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TdLib;

namespace sakuragram.Services;

public class MessageService
{
    private static TdClient _client = App._client;
    private List<long> _selectedMessages = [];
    public bool _isMessageSelected = false;

    public long[] GetSelectedMessages()
    {
        return _selectedMessages.ToArray();
    }

    public void SelectMessage(long messageId)
    {
        if (_selectedMessages.Contains(messageId)) return;
        _selectedMessages.Add(messageId);
        if (_selectedMessages.Count > 0) _isMessageSelected = true;
    }
    
    public void DeselectMessage(long messageId)
    {
        if (!_selectedMessages.Contains(messageId)) return;
        _selectedMessages.Remove(messageId);
        if (_selectedMessages.Count <= 0) _isMessageSelected = false;
    }

    public void ClearSelectedMessages()
    {
        if (_selectedMessages.Count <= 0) return;
        _selectedMessages.Clear();
    }
    
    public static Task<string> GetLastMessageContent(TdApi.Message message)
    {
        var lastMessage = message.Content switch
        {
            TdApi.MessageContent.MessageText messageText => $"{messageText.Text.Text}",
            TdApi.MessageContent.MessageAnimation messageAnimation => 
                $"GIF ({messageAnimation.Animation.Duration} sec), {messageAnimation.Caption.Text}",
            TdApi.MessageContent.MessageAudio messageAudio =>
                $"Audio message ({messageAudio.Audio.Duration} sec) {messageAudio.Caption.Text}",
            TdApi.MessageContent.MessageVoiceNote messageVoiceNote =>
                $"Voice message ({messageVoiceNote.VoiceNote.Duration} sec) {messageVoiceNote.Caption.Text}",
            TdApi.MessageContent.MessageVideo messageVideo =>
                $"Video ({messageVideo.Video.Duration} sec)",
            TdApi.MessageContent.MessageVideoNote messageVideoNote =>
                $"Video message ({messageVideoNote.VideoNote.Duration} sec)",
            TdApi.MessageContent.MessagePhoto messagePhoto =>
                $"Photo, {messagePhoto.Caption.Text}",
            TdApi.MessageContent.MessageSticker messageSticker =>
                $"{messageSticker.Sticker.Emoji} Sticker message",
            TdApi.MessageContent.MessagePoll messagePoll => $"📊 {messagePoll.Poll.Question.Text}",
            TdApi.MessageContent.MessagePinMessage messagePinMessage =>
                $"pinned",
            
            // Chat messages
            TdApi.MessageContent.MessageChatAddMembers messageChatAddMembers => $"{messageChatAddMembers.MemberUserIds}",
            TdApi.MessageContent.MessageChatChangeTitle messageChatChangeTitle => $"changed chat title to {messageChatChangeTitle.Title}",
            TdApi.MessageContent.MessageChatChangePhoto => "updated group photo",
            
            TdApi.MessageContent.MessageChatDeleteMember messageChatDeleteMember => 
                $"removed user {_client.GetUserAsync(userId: messageChatDeleteMember.UserId).Result.FirstName}",
            TdApi.MessageContent.MessageChatDeletePhoto => $"deleted group photo",
            
            TdApi.MessageContent.MessageChatUpgradeFrom messageChatUpgradeFrom => 
                $"{messageChatUpgradeFrom.Title} upgraded to supergroup",
            TdApi.MessageContent.MessageChatUpgradeTo messageChatUpgradeTo => $"",
            
            TdApi.MessageContent.MessageChatJoinByLink => $"joined by link",
            TdApi.MessageContent.MessageChatJoinByRequest => $"joined by request",
            
            _ => "Unsupported message type"
        };

        return Task.FromResult(lastMessage);
    }
}