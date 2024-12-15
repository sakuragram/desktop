using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Controls.Messages;
using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class StickerType
{
    private readonly TdClient _client = App._client;
    
    public StickerType(TdApi.Sticker sticker)
    {
        InitializeComponent();

        if (sticker == null) return;

        DispatcherQueue.EnqueueAsync(async () =>
        {
            Width = sticker.Width / 3.0;
            Height = sticker.Height / 3.0;
            
            if (sticker.Sticker_.Local.IsDownloadingCompleted) SetSticker(sticker.Sticker_.Local, sticker.Format);
            else
            {
                MediaLoading.Visibility = Visibility.Visible;
                var file = await _client.DownloadFileAsync(sticker.Sticker_.Id, Constants.MediaPriority, synchronous: true);
                SetSticker(file.Local, sticker.Format);
                MediaLoading.Visibility = Visibility.Collapsed;
            }
        });

        void SetSticker(TdApi.LocalFile file, TdApi.StickerFormat stickerFormat)
        {
            switch (stickerFormat)
            {
                case TdApi.StickerFormat.StickerFormatWebp:
                {
                    StaticSticker.Source = new BitmapImage(new Uri(file.Path));
                    StaticSticker.PointerPressed += async (_, _) => await OpenStickerSet(sticker);
                    StaticSticker.Visibility = Visibility.Visible;
                    break;
                }
                case TdApi.StickerFormat.StickerFormatTgs or TdApi.StickerFormat.StickerFormatWebm:
                {
                    AnimatedSticker.Source = MediaSource.CreateFromUri(new Uri(file.Path));
                    AnimatedSticker.MediaPlayer.AutoPlay = true;
                    AnimatedSticker.MediaPlayer.IsLoopingEnabled = true;
                    AnimatedSticker.MediaPlayer.Play();
                    AnimatedSticker.PointerPressed += async (_, _) => await OpenStickerSet(sticker);
                    AnimatedSticker.Visibility = Visibility.Visible;
                    break;
                }
            }
        }
    }

    private async Task OpenStickerSet(TdApi.Sticker sticker)
    {
        var stickerSet = await _client.GetStickerSetAsync(sticker.SetId);
        if (stickerSet == null) return;
        
        var stickerSetDialog = new ContentDialog { Title = stickerSet.Title, XamlRoot = this.XamlRoot, 
            PrimaryButtonText = stickerSet.IsInstalled ? "Share stickers" : "Add stickers", CloseButtonText = "Close", 
            DefaultButton = ContentDialogButton.Primary };
        var gridStickers = new Grid();
        var scrollViewer = new ScrollViewer { Content = gridStickers, MaxHeight = 250};

        if (!stickerSet.IsInstalled)
            stickerSetDialog.PrimaryButtonClick += async (_, _) => await _client.ChangeStickerSetAsync(stickerSet.Id, true);
        
        int favoriteRow = 0;
        int favoriteCol = 0;
        
        foreach (var stickerFromSet in stickerSet.Stickers)
        {
            var newSticker = new Sticker(stickerFromSet);

            if (favoriteCol == 5)
            {
                favoriteCol = 0;
                favoriteRow++;
                if (favoriteRow >= gridStickers.RowDefinitions.Count)
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                        gridStickers.RowDefinitions.Add(new RowDefinition { }));
                }
            }

            if (favoriteCol >= gridStickers.ColumnDefinitions.Count)
            {
                await DispatcherQueue.EnqueueAsync(() => gridStickers.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(64, GridUnitType.Auto) }));
            }

            await DispatcherQueue.EnqueueAsync(() =>
            {
                gridStickers.Children.Add(newSticker);
                Grid.SetRow(newSticker, favoriteRow);
                Grid.SetColumn(newSticker, favoriteCol);
            });
            favoriteCol++;
        }
        
        stickerSetDialog.Content = scrollViewer;
        await stickerSetDialog.ShowAsync();
    }
}