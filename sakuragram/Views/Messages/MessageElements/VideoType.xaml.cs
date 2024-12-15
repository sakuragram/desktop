using System;
using Windows.Media.Core;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class VideoType
{
    private readonly TdClient _client = App._client;
    
    public VideoType(TdApi.Video video)
    {
        InitializeComponent();
        
        if (video == null) return;

        DispatcherQueue.EnqueueAsync(async () =>
        {
            Width = video.Width / 2.5;
            Height = video.Height / 2.5;
            
            if (video.Video_.Local.IsDownloadingCompleted) SetVideo(video.Video_.Local);
            else
            {
                MediaLoading.Visibility = Visibility.Visible;
                var file = await _client.DownloadFileAsync(video.Video_.Id, Constants.MediaPriority, synchronous: true);
                SetVideo(file.Local);
                MediaLoading.Visibility = Visibility.Collapsed;
            }

            void SetVideo(TdApi.LocalFile file)
            {
                MediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(file.Path));
                MediaPlayerElement.MediaPlayer.IsMuted = true;
                MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
                MediaPlayerElement.MediaPlayer.AutoPlay = true;
                MediaPlayerElement.MediaPlayer.Play();
            }
        });
    }
}