using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class AnimationType
{
    public AnimationType(TdApi.Animation animation)
    {
        InitializeComponent();
        
        Width = animation.Width / 2.5;
        Height = animation.Height / 2.5;
    }
}