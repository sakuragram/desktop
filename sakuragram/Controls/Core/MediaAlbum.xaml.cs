using System.Collections.Generic;
using Microsoft.UI.Xaml;
using TdLib;

namespace sakuragram.Controls.Core;

public partial class MediaAlbum
{
    public MediaAlbum(bool needToShowPips = true)
    {
        InitializeComponent();
        
        if (!needToShowPips) PipsPager.Visibility = Visibility.Collapsed;
    }
    
    public void AddAlbumElement(object message) => FlipView.Items.Add(message);
}