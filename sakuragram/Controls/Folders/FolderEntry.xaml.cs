using System;
using TdLib;

namespace sakuragram.Controls.Folders;

public partial class FolderEntry
{
    public FolderEntry(TdApi.ChatFolderInfo info)
    {
        InitializeComponent();
        
        if (info == null)
        {
            Icon.Glyph = Constants.FolderIcon[3].Item2;
            TextBlockFolderName.Text = "All chats";
            return;
        }
        
        var folderIcon = Array.Find(Constants.FolderIcon, item => 
            item.Item1.Equals(info.Icon.Name, StringComparison.CurrentCultureIgnoreCase));
        if (folderIcon != null) Icon.Glyph = folderIcon.Item2;
        TextBlockFolderName.Text = info.Title;
        Tag = $"{info.Title}_{info.Id}";
    }
}