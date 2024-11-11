using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace sakuragram.Controls.Messages;

public class Reaction : Button
{
    public Reaction(TdApi.MessageReaction reaction)
    {
        CornerRadius = new CornerRadius(10);
        
        switch (reaction.Type)
        {
            case TdApi.ReactionType.ReactionTypeEmoji emoji:
            {
                var text = new TextBlock
                {
                    Text = emoji.Emoji,
                    FontSize = 10,
                    Margin = new Thickness(3, 0, 3, 0)
                };
                
                Content = text;
                break;
            }
            case TdApi.ReactionType.ReactionTypeCustomEmoji customEmoji:
            {
                break;
            }
            case TdApi.ReactionType.ReactionTypePaid paid:
            {
                
                break;
            }
        }
    }
}