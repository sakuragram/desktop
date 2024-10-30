using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Controls.Messages;

public class Sticker : Button
{
    private static readonly TdClient _client = App._client;
    private static TdApi.Sticker _sticker;
    private static ChatService _chatService = App.ChatService;
    private Image StickerImage { get; }
    
    public Sticker(TdApi.Sticker sticker)
    {
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        _sticker = sticker;
        
        Width = 64;
        Height = 64;
        
        StickerImage = new();
        if (sticker.Sticker_.Local.Path != string.Empty)
            StickerImage.Source = new BitmapImage(new Uri(sticker.Sticker_.Local.Path));
        else
            Task.Run(async () => await _client.DownloadFileAsync(fileId: sticker.Sticker_.Id, priority: 10));
        
        Content = StickerImage;
        Click += OnClick;
    }

    private Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
                if (updateFile.File.Id != _sticker.Sticker_.Id) return Task.CompletedTask;
                StickerImage.Source = updateFile.File.Local.Path != string.Empty ?
                    new BitmapImage(new Uri(updateFile.File.Local.Path)) : 
                    new BitmapImage(new Uri(_sticker.Sticker_.Local.Path));
                break;
        }
        return Task.CompletedTask;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        _client.ExecuteAsync(new TdApi.SendMessage
        {
            ChatId = _chatService._openedChatId,
            InputMessageContent = new TdApi.InputMessageContent.InputMessageSticker
            {
                Sticker = new TdApi.InputFile.InputFileRemote { Id = _sticker.Sticker_.Remote.Id }
            }
        });
    }
}