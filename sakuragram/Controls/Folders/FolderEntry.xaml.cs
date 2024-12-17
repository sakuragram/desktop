using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using TdLib;

namespace sakuragram.Controls.Folders;

public partial class FolderEntry
{
    private TdClient _client = App._client;
    private List<TdApi.Chat> _unreadedChats = [];
    
    public FolderEntry(TdApi.ChatFolderInfo info)
    {
        InitializeComponent();

        DispatcherQueue.EnqueueAsync(async () =>
        {
            if (info == null)
            {
                Icon.Glyph = Constants.FolderIcon[3].Item2;
                TextBlockFolderName.Text = "All chats";
                FlyoutItemEdit.Text = $@"Edit folders";
                FlyoutItemDelete.Visibility = Visibility.Collapsed;
                return;
            }
            
            await _client.LoadChatsAsync(new TdApi.ChatList.ChatListFolder { ChatFolderId = info.Id }, 100);
            
            var folderIcon = Array.Find(Constants.FolderIcon, item =>
                item.Item1.Equals(info.Icon.Name, StringComparison.CurrentCultureIgnoreCase));
            if (folderIcon != null) Icon.Glyph = folderIcon.Item2;
            TextBlockFolderName.Text = info.Title;
            Tag = $"{info.Title}_{info.Id}";
            FlyoutItemEdit.Text = $@"Edit {info.Title}";
            FlyoutItemDelete.Click += async (_, _) => await _client.DeleteChatFolderAsync(info.Id, []);
            FlyoutItemMarkAs.Click += async (_, _) =>
            {
                if (_unreadedChats.Count > 0)
                    await _client.ReadChatListAsync(new TdApi.ChatList.ChatListFolder { ChatFolderId = info.Id });
            };
        });
        
        _client.UpdateReceived += async (_, update) =>
        {
            switch (update)
            {
                case TdApi.Update.UpdateNewChat updateNewChat:
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        if (updateNewChat.Chat.ChatLists == null) return;
                        foreach (var chatList in updateNewChat.Chat.ChatLists)
                        {
                            if (chatList is TdApi.ChatList.ChatListFolder folder && folder.ChatFolderId != info.Id) continue;
                            if (updateNewChat.Chat is { UnreadCount: > 0 }) _unreadedChats.Add(updateNewChat.Chat);
                            if (_unreadedChats.Count > 0)
                            {
                                InfoBadgeUnreadChats.Value = _unreadedChats.Count;
                                InfoBadgeUnreadChats.Visibility = Visibility.Visible;
                            }
                        }
                    });
                    break;
            }
        };
    }
}