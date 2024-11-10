using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Controls.Messages;

public class Animation : Button
{
    private TdClient _client = App._client;
    private TdApi.Animation _animation;
    private static ChatService _chatService = App.ChatService;
    private MediaPlayerElement _mediaPlayerElement;
    
    public Animation(TdApi.Animation animation)
    {
        _animation = animation;
        
        Background = new SolidColorBrush(Colors.Transparent);
        Style = null;
        Margin = new Thickness(0);
        Padding = new Thickness(0);
        Width = 128;
        Height = 64;
        
        _mediaPlayerElement = new MediaPlayerElement();
        _mediaPlayerElement.Width = 128;
        _mediaPlayerElement.Height = 64;
        
        if (animation.Animation_.Local.IsDownloadingCompleted)
        {
            DispatcherQueue.EnqueueAsync(() => 
                _mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(animation.Animation_.Local.Path)));
        }
        else
        {
            Task.Run(async () =>
            {
                var file = await _client.DownloadFileAsync(animation.Animation_.Id, 10);
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    if (file.Local.IsDownloadingCompleted)
                        _mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(file.Local.Path));
                    else if (animation.Animation_.Local.IsDownloadingCompleted)
                        _mediaPlayerElement.Source =
                            MediaSource.CreateFromUri(new Uri(animation.Animation_.Local.Path));
                });
            });
            
        }
        
        Content = _mediaPlayerElement;
        Click += OnClick;
    }

    private async void OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = _chatService._openedChatId,
                InputMessageContent = new TdApi.InputMessageContent.InputMessageAnimation
                {
                    Animation = new TdApi.InputFile.InputFileRemote
                    {
                        Id = _animation.Animation_.Remote.Id
                    },
                    Thumbnail = new TdApi.InputThumbnail
                    {
                        Thumbnail = new TdApi.InputFile.InputFileRemote
                        {
                            Id = _animation.Thumbnail.File.Remote.Id
                        },
                        Width = _animation.Thumbnail.Width,
                        Height = _animation.Thumbnail.Height
                    },
                    Duration = _animation.Duration,
                    Width = _animation.Width,
                    Height = _animation.Height
                }
            });
        }
        catch (TdException exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}