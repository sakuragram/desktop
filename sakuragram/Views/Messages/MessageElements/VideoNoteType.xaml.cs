using System;
using Windows.Media.Core;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class VideoNoteType
{
    private readonly TdClient _client = App._client;
    
    public VideoNoteType(TdApi.VideoNote videoNote)
    {
        InitializeComponent();
        
        if (videoNote == null) return;

        DispatcherQueue.EnqueueAsync(async () =>
        {
            if (videoNote.Video.Local.IsDownloadingCompleted) SetVideo(videoNote.Video.Local);
            else
            {
                MediaLoading.Visibility = Visibility.Visible;
                var file = await _client.DownloadFileAsync(videoNote.Video.Id, Constants.MediaPriority, synchronous: true);
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