using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class AnimationType
{
    private TdClient _client = App._client;
    private TdApi.Animation _animation;
    
    public AnimationType(TdApi.Animation animation)
    {
        InitializeComponent();

        if (animation == null) return;
        _animation = animation;
        
        DispatcherQueue.EnqueueAsync(async () =>
        {
            Width = animation.Width / 3.0;
            Height = animation.Height / 3.0;

            if (animation.Animation_.Local.IsDownloadingCompleted) SetAnimation(animation.Animation_.Local);
            else
            {
                MediaLoading.Visibility = Visibility.Visible;
                var file = await _client.DownloadFileAsync(animation.Animation_.Id, Constants.MediaPriority, synchronous: true);
                SetAnimation(file.Local);
                MediaLoading.Visibility = Visibility.Collapsed;
            }
        });

        void SetAnimation(TdApi.LocalFile file)
        {
            MediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(file.Path));
            MediaPlayerElement.MediaPlayer.IsMuted = true;
            MediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;
            MediaPlayerElement.MediaPlayer.AutoPlay = true;
            MediaPlayerElement.MediaPlayer.Play();
        }
    }
}