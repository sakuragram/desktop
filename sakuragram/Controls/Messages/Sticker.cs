using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Core;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Controls.Messages;

public class Sticker : Button
{
    private static readonly TdClient _client = App._client;
    private readonly TdApi.Sticker _sticker;
    private static readonly ChatService _chatService = App.ChatService;
    private Image StickerImage { get; set; }
    private MediaPlayerElement StickerVideo { get; set; }

    private const int ButtonWidthAndHeight = 64;
    private const int StickerWidthAndHeight = 58;

    public Sticker(TdApi.Sticker sticker)
    {
        if (sticker == null) return;
        
        _sticker = sticker;

        DispatcherQueue.EnqueueAsync(async () =>
        {
            Background = new SolidColorBrush(Colors.Transparent);
            Style = null;
            Margin = new Thickness(3);
            Padding = new Thickness(0);
            Width = ButtonWidthAndHeight;
            Height = ButtonWidthAndHeight;

            if (sticker.Sticker_.Local.IsDownloadingCompleted) await SetSticker(sticker.Sticker_.Local, sticker.Format);
            else
            {
                var file = await _client.DownloadFileAsync(sticker.Sticker_.Id, Constants.MediaPriority,
                    synchronous: true);
                await SetSticker(file.Local, sticker.Format);
            }
            
            Click += OnClick;
        });
        return;

        async Task SetSticker(TdApi.LocalFile file, TdApi.StickerFormat format)
        {
            switch (format)
            {
                case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        StickerVideo = new MediaPlayerElement
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Width = StickerWidthAndHeight,
                            Height = StickerWidthAndHeight,
                            Source = MediaSource.CreateFromUri(new Uri(file.Path)),
                            MediaPlayer =
                            {
                                IsLoopingEnabled = true,
                                Position = TimeSpan.Zero,
                                AutoPlay = true
                            }
                        };
                        StickerVideo.MediaPlayer.Play();
                        Content = StickerVideo;
                    });
                    break;
                case TdApi.StickerFormat.StickerFormatWebp:
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        StickerImage = new Image
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Width = StickerWidthAndHeight,
                            Height = StickerWidthAndHeight,
                            Source = new BitmapImage(new Uri(file.Path))
                        };
                        Content = StickerImage;
                    });
                    break;
            }
        }
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