using System;
using System.IO;
using System.Diagnostics;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace sakuragram.Views.Settings;

public partial class Folders : Page
{
    private readonly TdClient _client = App._client;
    private TdApi.ChatFolderInfo[] _chatFolders = App._folders;
    private bool _hasInternetConnection = App._hasInternetConnection;
    
    public Folders()
    {
        InitializeComponent();
        
        GenerateFolders(_hasInternetConnection);
    }
    
    private void GenerateFolders(bool hasInternetConnection)
    {
        PanelUserFolders.Children.Clear();
        
        foreach (var userFolder in _chatFolders)
        {
            string path = AppContext.BaseDirectory +
                          $@"Assets\icons\folders\folders_{userFolder.Icon.Name.ToLower()}.png";
            BitmapIcon folderIcon = new();
					
            if (File.Exists(path))
            {
                Uri folderIconPath = new(path);
						
                folderIcon.UriSource = folderIconPath;
                folderIcon.ShowAsMonochrome = false;
            }
            
            TdApi.ChatFolder chatFolderInfo = _client.GetChatFolderAsync(userFolder.Id).Result;

            Button button = new();
            button.Content = "Delete";
            button.Click += (sender, args) =>
            {
                _client.DeleteChatFolderAsync(userFolder.Id, []);
                GenerateFolders(_hasInternetConnection);
            };
            
            SettingsCard card = new();
            card.Header = userFolder.Title;
            //card.HeaderIcon = folderIcon != null ? folderIcon : null;
            card.Description = chatFolderInfo.IncludedChatIds.Length + " chats";
            card.Content = button;
            
            PanelUserFolders.Children.Add(card);
        }

        if (hasInternetConnection)
        {
            var recommendedFolders = _client.ExecuteAsync(new TdApi.GetRecommendedChatFolders()).Result;
            
            foreach (var folder in recommendedFolders.ChatFolders)
            {
                SettingsCard card = new();
                card.Header = folder.Folder.Title;
                card.Description = folder.Description;

                Button button = new();
                button.Content = "Add";
                button.Click += async (sender, args) =>
                {
                    await _client.CreateChatFolderAsync(folder.Folder);
                    GenerateFolders(_hasInternetConnection);
                };
                
                card.Content = button;
                PanelUserRecommendedFolders.Children.Add(card);
            }
        }
    }
}