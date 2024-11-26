using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class VideoType
{
    public VideoType(TdApi.Video video)
    {
        InitializeComponent();
        
        Width = video.Width / 2.5;
        Height = video.Height / 2.5;
    }
}