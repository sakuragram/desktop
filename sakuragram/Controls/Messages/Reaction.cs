using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TdLib;

namespace sakuragram.Controls.Messages;

public class Reaction : Button
{
    private TdClient _client = App._client;
    
    public Reaction(TdApi.MessageReaction reaction, long chatId, long messageId)
    {
        CornerRadius = new CornerRadius(12);
        Padding = new Thickness(0);
        Margin = new Thickness(0, 0, 2, 0);
        Width = 55;
        BorderBrush = null;
        BorderThickness = new Thickness(0);
        
        StackPanel stackPanel = new();
        stackPanel.Orientation = Orientation.Horizontal;
        
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
                var textCount = new TextBlock
                {
                    Text = reaction.TotalCount.ToString(),
                    FontSize = 12,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 2, 5, 2)
                };
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
                var customEmojiAnimation = _client.GetCustomEmojiReactionAnimationsAsync();
                break;
            }
            case TdApi.ReactionType.ReactionTypePaid paid:
            {
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