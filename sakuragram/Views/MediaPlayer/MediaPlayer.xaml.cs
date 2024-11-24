using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using TdLib;

namespace sakuragram.Views.MediaPlayer;

public partial class MediaPlayer
{
    private TdClient _client = App._client;
    
    private object _media;
    private int _mediaIndex = 0;
    private int _fileId;
    
    private TdApi.StoryContent _storyContent;
    
    public MediaPlayer(object media)
    {
        InitializeComponent();
        MediaPlayerElement.MediaPlayer.Volume = 0.1;
        MediaPlayerElement.TransportControls.ShowAndHideAutomatically = true;
        MediaPlayerElement.TransportControls.IsCompact = true;
        // ExtendsContentIntoTitleBar = true;
        // AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        // Title = "Media viewer";
        // SystemBackdrop = new DesktopAcrylicBackdrop();
        // AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        PrepareMedia(media);
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id == _fileId)
                {
                    if (updateFile.File.Local.IsDownloadingCompleted)
                        switch (_storyContent)
                        {
                            case TdApi.StoryContent.StoryContentPhoto storyContentPhoto:
                                if (updateFile.File.Local.IsDownloadingCompleted)
                                    await DispatcherQueue.EnqueueAsync(() => MediaPlayerElement.Source =
                                        MediaSource.CreateFromUri(
                                            new Uri(updateFile.File.Local.Path)));
                                else
                                    await DispatcherQueue.EnqueueAsync(() => MediaPlayerElement.Source =
                                        MediaSource.CreateFromUri(
                                            new Uri(storyContentPhoto.Photo.Sizes[0].Photo.Local.Path)));
                                break;
                            case TdApi.StoryContent.StoryContentVideo storyContentVideo:
                                if (updateFile.File.Local.IsDownloadingCompleted)
                                    await DispatcherQueue.EnqueueAsync(() =>
                                    {
                                        MediaPlayerElement.Source =
                                            MediaSource.CreateFromUri(
                                                new Uri(updateFile.File.Local.Path));
                                        MediaPlayerElement.MediaPlayer.Play();
                                    });
                                else
                                    await DispatcherQueue.EnqueueAsync(() =>
                                    {
                                        MediaPlayerElement.Source =
                                            MediaSource.CreateFromUri(
                                                new Uri(storyContentVideo.Video.Video.Local.Path));
                                        MediaPlayerElement.MediaPlayer.Play();
                                    });
                                break;
                        }
                }
                break;
            }
        }
    }

    private async void PrepareMedia(object media)
    {
        _media = media;
        
        if (media is TdApi.ChatActiveStories chatActiveStories)
        {
            var activeStory = chatActiveStories.Stories[_mediaIndex];
            var story = await _client.GetStoryAsync(chatActiveStories.ChatId, activeStory.StoryId);
            
            if (story != null)
            {
                _storyContent = story.Content;
                switch (story.Content)
                {
                    case TdApi.StoryContent.StoryContentUnsupported:
                        TextBlockTdException.Text = "Story is unsupported with this version of TDLib";
                        TextBlockTdException.Visibility = Visibility.Visible;
                        break;
                    case TdApi.StoryContent.StoryContentPhoto storyContentPhoto:
                        _fileId = storyContentPhoto.Photo.Sizes[0].Photo.Id;

                        if (storyContentPhoto.Photo.Sizes[0].Photo.Local.IsDownloadingCompleted)
                            MediaPlayerElement.Source = MediaSource.CreateFromUri(
                                new Uri(storyContentPhoto.Photo.Sizes[0].Photo.Local.Path));
                        else
                            await _client.DownloadFileAsync(_fileId, 1);
                        break;
                    case TdApi.StoryContent.StoryContentVideo storyContentVideo:
                        _fileId = storyContentVideo.Video.Video.Id;

                        if (storyContentVideo.Video.Video.Local.IsDownloadingCompleted)
                            MediaPlayerElement.Source = MediaSource.CreateFromUri(
                                new Uri(storyContentVideo.Video.Video.Local.Path));
                        else
                        {
                            await _client.DownloadFileAsync(_fileId, 1, synchronous: true);
                            MediaPlayerElement.MediaPlayer.Play();
                        }
                        break;
                }
            }
        }
    }

    private void UIElement_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Escape:
                Close();
                break;
            case VirtualKey.Right:
                switch (_media)
                {
                    case TdApi.ChatActiveStories chatActiveStories:
                        _mediaIndex++;
                        PrepareMedia(chatActiveStories);
                        break;
                }
                break;
            case VirtualKey.Left:
                switch (_media)
                {
                    case TdApi.ChatActiveStories chatActiveStories:
                        _mediaIndex--;
                        PrepareMedia(chatActiveStories);
                        break;
                }
                break;
        }
    }

    private void MediaPlayer_OnClosed(object sender, WindowEventArgs args)
    {
        MediaPlayerElement.MediaPlayer.Pause();
    }
}