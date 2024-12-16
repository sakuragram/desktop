using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;
using Brush = Microsoft.UI.Xaml.Media.Brush;

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
    
    private async void GenerateFolders(bool hasInternetConnection)
    {
        PanelUserFolders.Children.Clear();
        
        foreach (var userFolder in _chatFolders)
        {
            TdApi.ChatFolder chatFolderInfo = await _client.GetChatFolderAsync(userFolder.Id);
            var folderChatCount = await _client.GetChatFolderChatCountAsync(chatFolderInfo);
            var folderIcon = Array.Find(Constants.FolderIcon, item =>
                item.Item1.Equals(userFolder.Icon.Name, StringComparison.CurrentCultureIgnoreCase));

            StackPanel stackPanel = new();
            stackPanel.Orientation = Orientation.Horizontal;
            
            Button buttonDelete = new();
            buttonDelete.Content = new FontIcon { Glyph = "\uE74D" };
            buttonDelete.Click += async (_, _) =>
            {
                await _client.DeleteChatFolderAsync(userFolder.Id, []);
                GenerateFolders(_hasInternetConnection);
            };
            
            Button buttonEdit = new();
            buttonEdit.Content = new FontIcon { Glyph = "\uE70F" };
            buttonEdit.Margin = new Thickness(0, 0, 5, 0);
            buttonEdit.Click += async (_, _) =>
            {
                TextBoxEditFolderName.Text = userFolder.Title;
                await DialogEditFolder.ShowAsync();
                DialogEditFolder.PrimaryButtonClick += async (_, _) =>
                {
                    await _client.EditChatFolderAsync(userFolder.Id, new TdApi.ChatFolder
                    {
                        Title = TextBoxEditFolderName.Text
                    });
                };
            };
            
            SettingsCard card = new();
            card.Header = userFolder.Title;
            card.HeaderIcon = new FontIcon { Glyph = folderIcon.Item2 };
            card.Description = folderChatCount.Count_ + " chats";
            
            stackPanel.Children.Add(buttonEdit);
            stackPanel.Children.Add(buttonDelete);
            card.Content = stackPanel;
            PanelUserFolders.Children.Add(card);
        }
        
        var recommendedFolders = _client.ExecuteAsync(new TdApi.GetRecommendedChatFolders()).Result;
        if (recommendedFolders == null)
        {
            TextBlockRecommendedFolders.Visibility = Visibility.Collapsed;
            return;
        }
        
        foreach (var folder in recommendedFolders.ChatFolders)
        {
            SettingsCard card = new();
            card.Header = folder.Folder.Title;
            card.Description = folder.Description;

            if (folder.Folder.Icon != null)
            {
                var folderIcon = Array.Find(Constants.FolderIcon, item =>
                    item.Item1.Equals(folder.Folder.Icon.Name, StringComparison.CurrentCultureIgnoreCase));
                card.HeaderIcon = new FontIcon { Glyph = folderIcon.Item2 };
            }
            
            Button button = new();
            button.Style = (Style)Application.Current.Resources["AccentButtonStyle"]; 
            button.Content = new FontIcon { Glyph = "\uE710" };
            button.Click += async (_, _) =>
            {
                await _client.CreateChatFolderAsync(folder.Folder);
                GenerateFolders(_hasInternetConnection);
            };

            card.Content = button;
            PanelUserRecommendedFolders.Children.Add(card);
        }
    }
}