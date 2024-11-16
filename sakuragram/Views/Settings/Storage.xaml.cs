using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using sakuragram.Views.Settings.AdditionalElements;
using TdLib;

namespace sakuragram.Views.Settings;

public partial class Storage : Page
{
    private TdClient _client = App._client;
    public Frame Frame { get; set; }
    
    public Storage()
    {
        InitializeComponent();
        
        Task.Run(async () =>
        {
            var storageUsageFast = await _client.GetStorageStatisticsFastAsync();
            var storageUsage = await _client.GetStorageStatisticsAsync();
            
            await DispatcherQueue.EnqueueAsync(() =>
            {
                CardDatabaseSize.Description = $"Database size: {storageUsageFast.DatabaseSize / 1024 / 1024} MB";
                CardLogSize.Description = $"Log size: {storageUsageFast.LogSize / 1024 / 1024} MB";
                CardFilesSize.Description = $"Files size: {storageUsageFast.FilesSize / 1024 / 1024} MB ({storageUsageFast.FileCount} files)";
                
                ExpanderStorageByChat.Description = $"{storageUsage.ByChat.Length} chats";

                foreach (var statisticsByChat in storageUsage.ByChat)
                {
                    SettingsCard card = new();
                    card.Header = statisticsByChat.ChatId;
                    card.Description = $"Database size: {statisticsByChat.Size / 1024 / 1024} MB";
                    card.IsClickEnabled = true;
                    card.Click += async (_, _) =>
                    {
                        Frame.Navigate(typeof(StorageChatPage));
                        if (Frame.Content is StorageChatPage page) await page.InitializePage(statisticsByChat);
                    };
                    
                    ExpanderStorageByChat.Items.Add(card);
                }
            });
        });
    }

    private async void ButtonOptimizeStorage_OnClick(object sender, RoutedEventArgs e)
    {
        await _client.ExecuteAsync(new TdApi.OptimizeStorage());
    }
}