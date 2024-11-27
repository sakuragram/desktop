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
    private bool _isDownloaded;
    private string _uri;
    
    public AnimationType(TdApi.Animation animation)
    {
        InitializeComponent();

        _animation = animation;
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        
        DispatcherQueue.EnqueueAsync(async () =>
        {
            Width = animation.Width / 2.5;
            Height = animation.Height / 2.5;

            if (animation.Animation_.Local.IsDownloadingCompleted)
            {
                MediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(animation.Animation_.Local.Path));
                MediaLoading.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (animation.Animation_.Local.CanBeDownloaded)
                    await _client.DownloadFileAsync(animation.Animation_.Id, 3, synchronous: false);
                if (_isDownloaded)
                    Console.WriteLine("s");
            }
        });
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
                if (updateFile.File.Id == _animation.Animation_.Id)
                {
                    if (string.IsNullOrEmpty(updateFile.File.Local.Path) || updateFile.File.Local.Path == string.Empty ||
                        !updateFile.File.Local.IsDownloadingCompleted) return;
                    _isDownloaded = updateFile.File.Local.IsDownloadingCompleted;
                    _uri = updateFile.File.Local.Path;
                    
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        MediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri(_isDownloaded ? _uri : string.Empty));
                        MediaLoading.Visibility = _isDownloaded ? Visibility.Collapsed : Visibility.Visible;
                    });
                }
                break;
        }
    }
}