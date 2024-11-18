using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace sakuragram.Views.Settings.AdditionalElements;

public partial class StorageChatPage : Page
{
    private readonly TdClient _client = App._client;

    public StorageChatPage()
    {
        InitializeComponent();
        
        ButtonReturn.Click += (_, _) =>
        {
            Frame.Navigate(typeof(Storage));
            var storagePage = (Storage)Frame.Content;
            storagePage.Frame = Frame;
        };
    }

    public async Task InitializePage(TdApi.StorageStatisticsByChat statisticsByChat)
    {
        try
        {
            var chat = await _client.GetChatAsync(statisticsByChat.ChatId);
            TextBlockChatTitle.Text = chat.Title;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TextBlockChatTitle.Text = "None";
        }
        
        foreach (var statisticsByFileType in statisticsByChat.ByFileType)
        {
            SettingsCard settingsCard = new();
            settingsCard.Header = statisticsByFileType.FileType switch
            {
                TdApi.FileType.FileTypeAnimation => "Animation",
                TdApi.FileType.FileTypeAudio => "Audio",
                TdApi.FileType.FileTypeDocument => "Document",
                TdApi.FileType.FileTypeNone => "None",
                TdApi.FileType.FileTypeNotificationSound => "Notification Sound",
                TdApi.FileType.FileTypePhoto => "Photo",
                TdApi.FileType.FileTypePhotoStory => "Photo Story",
                TdApi.FileType.FileTypeProfilePhoto => "Profile Photo",
                TdApi.FileType.FileTypeSecret => "Secret",
                TdApi.FileType.FileTypeSecretThumbnail => "Secret Thumbnail",
                TdApi.FileType.FileTypeSecure => "Secure",
                TdApi.FileType.FileTypeSticker => "Sticker",
                TdApi.FileType.FileTypeThumbnail => "Thumbnail",
                TdApi.FileType.FileTypeUnknown => "Unknown",
                TdApi.FileType.FileTypeVideo => "Video",
                TdApi.FileType.FileTypeVideoNote => "Video Note",
                TdApi.FileType.FileTypeVideoStory => "Video Story",
                TdApi.FileType.FileTypeVoiceNote => "Voice Note",
                TdApi.FileType.FileTypeWallpaper => "Wallpaper",
                _ => throw new ArgumentOutOfRangeException()
            };
            settingsCard.Description = $"{statisticsByFileType.Count} files, {statisticsByFileType.Size / 1024 / 1024} MB";
            PanelStorageFileTypes.Children.Add(settingsCard);
        }
    }
}