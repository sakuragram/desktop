﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
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
    
    public static async Task<string> GetTextMessageContent(TdApi.Message message)
    {
        if (message == null ) return null;
        var chat = await _client.GetChatAsync(message.ChatId);
        
        if (chat.DraftMessage != null)
        {
            string lastMessage = chat.DraftMessage.InputMessageText switch
            {
                TdApi.InputMessageContent.InputMessageText messageText => $"Draft: {messageText.Text.Text}",
                _ => "Draft message"
            };
            return lastMessage;
        }
        else
        {
            string lastMessage = message.Content switch
            {
                TdApi.MessageContent.MessageText messageText => $"{messageText.Text.Text}",
                
                TdApi.MessageContent.MessageVideoNote =>  "Video message",
                
                TdApi.MessageContent.MessageSticker messageSticker =>
                    $"{messageSticker.Sticker.Emoji} Sticker message",
                
                TdApi.MessageContent.MessagePoll messagePoll => $"📊 {messagePoll.Poll.Question.Text}",

                #region MessageContent with caption
                
                TdApi.MessageContent.MessageAnimation messageAnimation => messageAnimation.Caption.Text != string.Empty ? 
                    $"GIF, {messageAnimation.Caption.Text}" : "GIF",
                
                TdApi.MessageContent.MessageAudio messageAudio => messageAudio.Caption.Text != string.Empty ? 
                    $"{messageAudio.Audio.Title}, {messageAudio.Caption.Text}" : 
                    messageAudio.Audio.Title,
                
                TdApi.MessageContent.MessageVoiceNote messageVoiceNote => messageVoiceNote.Caption.Text != string.Empty ? 
                    $"Voice message, {messageVoiceNote.Caption.Text}" : "Voice message",
                
                TdApi.MessageContent.MessageVideo messageVideo => messageVideo.Caption.Text != string.Empty ?
                    $"Video, {messageVideo.Caption.Text}" : "Video",
                
                TdApi.MessageContent.MessagePhoto messagePhoto => messagePhoto.Caption.Text != string.Empty ?
                    $"Photo, {messagePhoto.Caption.Text}" : "Photo",
                
                TdApi.MessageContent.MessageDocument messageDocument => messageDocument.Caption.Text != string.Empty ?
                    $"{messageDocument.Document.FileName}, {messageDocument.Caption.Text}" : 
                    messageDocument.Document.FileName,
                
                TdApi.MessageContent.MessagePaidMedia messagePaidMedia => messagePaidMedia.Caption.Text != string.Empty ?
                    $"Paid Media, {messagePaidMedia.Caption.Text}" : "Paid Media",
                
                #endregion
                
                TdApi.MessageContent.MessagePinMessage messagePinMessage =>
                    $"pinned message: {messagePinMessage.MessageId}",
                
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
            return lastMessage;
        }
    }

    public static async Task<Span> GetTextEntities(TdApi.FormattedText formattedText)
    {
        var inlines = new Span();
        var text = formattedText.Text;
        int lastIndex = 0;

        foreach (var textEntity in formattedText.Entities)
        {
            switch (textEntity.Type)
            {
                case TdApi.TextEntityType.TextEntityTypeUrl:
                {
                    var regex = new Regex(@"(https?://[^\s]+)");
                    var match = regex.Match(text, lastIndex);

                    if (match.Success)
                    {
                        var link = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri(link),
                            Foreground = new SolidColorBrush(Colors.Azure),
                            TextDecorations = TextDecorations.Underline
                        };
                        hyperlink.Inlines.Add(new Run { Text = link });

                        inlines.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        inlines.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeTextUrl url:
                {
                    var hyperlink = new Hyperlink
                    {
                        NavigateUri = new Uri(url.Url),
                        Foreground = new SolidColorBrush(Colors.Azure),
                        TextDecorations = TextDecorations.Underline
                    };
                    hyperlink.Inlines.Add(new Run { Text = text });
                    
                    inlines.Inlines.Add(new Run { Text = text.Substring(lastIndex, textEntity.Length - lastIndex) });
                    inlines.Inlines.Add(hyperlink);
                    lastIndex = textEntity.Offset + textEntity.Length;
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeHashtag:
                {
                    var regex = new Regex(@"(#\w+)");
                    var match = regex.Match(text);
                    
                    if (match.Success)
                    {
                        var hashtag = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri("https://t.me/sakuragram/"),
                            Foreground = new SolidColorBrush(Colors.Azure)
                        };
                        hyperlink.Inlines.Add(new Run { Text = hashtag });
                        
                        inlines.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        inlines.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeMentionName mentionName:
                {
                    var regex = new Regex(@"(@\w+)");
                    var match = regex.Match(text);
                    
                    if (match.Success)
                    {
                        var mention = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri($"https://t.me/{mentionName.UserId}"),
                            Foreground = new SolidColorBrush(Colors.Azure)
                        };
                        hyperlink.Inlines.Add(new Run { Text = mention });
                        
                        inlines.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        inlines.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
                case TdApi.TextEntityType.TextEntityTypeMention:
                {
                    var regex = new Regex(@"(@\w+)");
                    var match = regex.Match(text);
                    
                    if (match.Success)
                    {
                        var mention = match.Value;
                        var hyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri($"https://t.me/{mention.Replace("@", string.Empty)}"),
                            Foreground = new SolidColorBrush(Colors.Azure)
                        };
                        hyperlink.Inlines.Add(new Run { Text = mention });
                        
                        inlines.Inlines.Add(new Run { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                        inlines.Inlines.Add(hyperlink);
                        lastIndex = match.Index + match.Length;
                    }
                    break;
                }
            }
        }
        
        inlines.Inlines.Add(new Run { Text = text.Substring(lastIndex) });
        
        return inlines;
    }
}