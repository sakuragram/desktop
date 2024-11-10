using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Controls.Messages;

public class Sticker : Button
{
    private static readonly TdClient _client = App._client;
    private TdApi.Sticker _sticker;
    private static ChatService _chatService = App.ChatService;
    private Image StickerImage { get; }
    private MediaPlayerElement StickerVideo { get; }
    
    public Sticker(TdApi.Sticker sticker)
    {
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        _sticker = sticker;
        
        Width = 64;
        Height = 64;

        switch (sticker.Format)
        {
            case TdApi.StickerFormat.StickerFormatWebp:
                StickerImage = new();
                if (sticker.Sticker_.Local.IsDownloadingCompleted)
                    StickerImage.Source = new BitmapImage(new Uri(sticker.Sticker_.Local.Path));
                else
                    Task.Run(async () => await _client.DownloadFileAsync(fileId: sticker.Sticker_.Id, priority: 10));
                Content = StickerImage;
                break;
            case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                StickerVideo = new();
                if (sticker.Sticker_.Local.IsDownloadingCompleted)
                {
                    StickerVideo.Source = MediaSource.CreateFromUri(new Uri(sticker.Sticker_.Local.Path));
                    StickerVideo.MediaPlayer.IsLoopingEnabled = true;
                    StickerVideo.MediaPlayer.Position = TimeSpan.Zero;
                    StickerVideo.MediaPlayer.AutoPlay = true;
                    StickerVideo.MediaPlayer.Play();
                }
                else
                    Task.Run(async () => await _client.DownloadFileAsync(fileId: sticker.Sticker_.Id, priority: 10));
                Content = StickerVideo;
                break;
        }
        
        // TextBlock debug = new();
        // debug.Text = sticker.Id.ToString();
        // Content = debug;
        
        Click += OnClick;
    }

    private Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
                if (updateFile.File.Id != _sticker.Sticker_.Id) return Task.CompletedTask;
                switch (_sticker.Format)
                {
                    case TdApi.StickerFormat.StickerFormatWebp:
                        if (updateFile.File.Local.IsDownloadingCompleted)
                            StickerImage.Source = updateFile.File.Local.IsDownloadingCompleted ?
                                new BitmapImage(new Uri(updateFile.File.Local.Path)) : 
                                new BitmapImage(new Uri(_sticker.Sticker_.Local.Path));
                        break;
                    case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                        if (updateFile.File.Local.IsDownloadingCompleted)
                        {
                            StickerVideo.Source = MediaSource.CreateFromUri(
                                new Uri(updateFile.File.Local.IsDownloadingCompleted
                                    ? updateFile.File.Local.Path
                                    : _sticker.Sticker_.Local.Path));
                            StickerVideo.MediaPlayer.IsLoopingEnabled = true;
                            StickerVideo.MediaPlayer.Position = TimeSpan.Zero;
                            StickerVideo.MediaPlayer.AutoPlay = true;
                            StickerVideo.MediaPlayer.Play();
                        }
                        break;
                }
                break;
        }
        return Task.CompletedTask;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = _chatService._openedChatId,
                InputMessageContent = new TdApi.InputMessageContent.InputMessageSticker
                {
                    Sticker = new TdApi.InputFile.InputFileRemote { Id = _sticker.Sticker_.Remote.Id },
                    Emoji = _sticker.Emoji,
                    Height = _sticker.Height,
                    Width = _sticker.Width
                }
            });
        }
        catch (TdException exception)
        {
            Debug.WriteLine(exception);
            throw;
        }
    }
}