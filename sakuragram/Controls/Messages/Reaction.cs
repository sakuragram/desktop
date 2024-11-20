using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;
using Brush = Microsoft.UI.Xaml.Media.Brush;

namespace sakuragram.Controls.Messages;

public class Reaction : Button
{
    private TdClient _client = App._client;
    private TdApi.Sticker _customEmojiSticker;
    
    private Image _customEmojiImage;
    private MediaPlayerElement _customEmojiPlayerElement;
    
    private bool _isAnimatedReaction;
    
    public Reaction(TdApi.MessageReaction reaction, long chatId, long messageId)
    {
        CornerRadius = new CornerRadius(6);
        Padding = new Thickness(0);
        Margin = new Thickness(0, 0, 2, 0);
        Width = 55;
        BorderBrush = null;
        BorderThickness = new Thickness(0);
        
        StackPanel stackPanel = new();
        stackPanel.Orientation = Orientation.Horizontal;

        var textCount = new TextBlock
        {
            FontSize = 12,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 2, 5, 2)
        };

        switch (reaction.Type)
        {
            case TdApi.ReactionType.ReactionTypeEmoji emoji:
            {
                var textEmoji = new TextBlock
                {
                    Text = emoji.Emoji,
                    FontSize = 14,
                    Padding = new Thickness(0),
                    Margin = new Thickness(5, 2, 5, 2)
                };
                
                textCount.Text = reaction.TotalCount.ToString();
                
                stackPanel.Children.Add(textEmoji);
                stackPanel.Children.Add(textCount);
                
                if (reaction.IsChosen) 
                    Background = (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorBaseBrush"];
                else
                    Background = (Brush)Application.Current.Resources["SolidBackgroundFillColorBaseAltBrush"];
                break;
            }
            case TdApi.ReactionType.ReactionTypeCustomEmoji customEmoji:
            {
                var customEmojiStickers = _client.GetCustomEmojiStickersAsync([customEmoji.CustomEmojiId]).Result;
                
                foreach (var sticker in customEmojiStickers.Stickers_)
                {
                    _customEmojiSticker = sticker;
                    
                    switch (sticker.Format)
                    {
                        case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                        {
                            _customEmojiImage = new Image
                            {
                                Width = 14,
                                Height = 14,
                                Margin = new Thickness(5, 2, 5, 2)
                            };
                            _isAnimatedReaction = false;
                            break;
                        }
                        case TdApi.StickerFormat.StickerFormatWebp:
                        {
                            _customEmojiPlayerElement = new MediaPlayerElement
                            {
                                Width = 14,
                                Height = 14,
                                Margin = new Thickness(5, 2, 5, 2),
                                AutoPlay = true,
                                MediaPlayer =
                                {
                                    IsLoopingEnabled = true,
                                    IsMuted = true
                                }
                            };
                            _isAnimatedReaction = true;
                            break;
                        }
                    }
                    
                    if (sticker.Sticker_.Local.IsDownloadingCompleted)
                    {
                        switch (sticker.Format)
                        {
                            case TdApi.StickerFormat.StickerFormatWebp:
                                if (_customEmojiImage == null) return;
                                _customEmojiImage.Source = new BitmapImage(new Uri(sticker.Sticker_.Local.Path));
                                break;
                            case TdApi.StickerFormat.StickerFormatWebm or TdApi.StickerFormat.StickerFormatTgs:
                                if (_customEmojiPlayerElement == null) return;
                                _customEmojiPlayerElement.Source = 
                                    MediaSource.CreateFromUri(new Uri(sticker.Sticker_.Local.Path));
                                break;
                        }
                    }
                    else
                    {
                        _client.DownloadFileAsync(sticker.Sticker_.Id, 10, synchronous: true);

                        if (_isAnimatedReaction)
                            DispatcherQueue.EnqueueAsync(() =>
                                _customEmojiPlayerElement.Source =
                                    MediaSource.CreateFromUri(new Uri(sticker.Sticker_.Local.Path)));
                        else
                            DispatcherQueue.EnqueueAsync(() =>
                                _customEmojiImage.Source = new BitmapImage(new Uri(sticker.Sticker_.Local.Path)));
                    }
                }
                
                textCount.Text = reaction.TotalCount.ToString();
                
                if (_isAnimatedReaction) stackPanel.Children.Add(_customEmojiPlayerElement);
                else stackPanel.Children.Add(_customEmojiImage);
                
                stackPanel.Children.Add(textCount);
                
                if (reaction.IsChosen) 
                    Background = (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorBaseBrush"];
                else
                    Background = (Brush)Application.Current.Resources["SolidBackgroundFillColorBaseAltBrush"];
                break;
            }
            case TdApi.ReactionType.ReactionTypePaid:
            {
                var textEmoji = new TextBlock
                {
                    Text = "⭐",
                    FontSize = 14,
                    Padding = new Thickness(0),
                    Margin = new Thickness(5, 2, 5, 2)
                };
                
                textCount.Text = reaction.TotalCount.ToString();
                textCount.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 186, 105, 32));
                textCount.FontSize = 12;
                textCount.Padding = new Thickness(0);
                textCount.Margin = new Thickness(0, 2, 5, 2);
                
                stackPanel.Children.Add(textEmoji);
                stackPanel.Children.Add(textCount);
                
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 73, 71, 56));
                break;
            }
        }
        
        Content = stackPanel;
        Click += async (_, _) =>
        {
            if (reaction.IsChosen)
            {
                try
                {
                    await _client.RemoveMessageReactionAsync(chatId, messageId, reaction.Type);
                    Background = (Brush)Application.Current.Resources["SolidBackgroundFillColorBaseAltBrush"];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                try
                {
                    await _client.AddMessageReactionAsync(chatId, messageId, reaction.Type, false, true);
                    Background = (Brush)Application.Current.Resources["AccentAcrylicBackgroundFillColorBaseBrush"];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        };
    }
}