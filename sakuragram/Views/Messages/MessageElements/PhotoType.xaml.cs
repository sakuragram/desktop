using System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class PhotoType
{
    private readonly TdClient _client = App._client;
    
    public PhotoType(TdApi.Photo photo)
    {
        InitializeComponent();
        
        if (photo == null) return;

        DispatcherQueue.EnqueueAsync(async () =>
        {
            Width = photo.Sizes[0].Width;
            Height = photo.Sizes[0].Height;

            if (photo.Sizes[0].Photo.Local.IsDownloadingCompleted) SetPhoto(photo.Sizes[0].Photo.Local);
            else
            {
                MediaLoading.Visibility = Visibility.Visible;
                var file = await _client.DownloadFileAsync(photo.Sizes[0].Photo.Id, Constants.MediaPriority, synchronous: true);
                SetPhoto(file.Local);
                MediaLoading.Visibility = Visibility.Collapsed;
            }

            void SetPhoto(TdApi.LocalFile file)
            {
                Image.Source = new BitmapImage(new Uri(file.Path));
            }
        });
    }
}