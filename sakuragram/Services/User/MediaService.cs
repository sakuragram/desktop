using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Controls.User;
using TdLib;

namespace sakuragram.Services;

public class MediaService
{
    private static TdClient _client = App._client;
    
    public static async Task GetChatPhoto(TdApi.Chat chat, ProfilePhoto avatar)
    {
        if (chat.Photo == null)
        {
            avatar._personPicture.DisplayName = chat.Title;
            return;
        }

        if (chat.Photo.Small.Local.IsDownloadingCompleted)
        {
            avatar._personPicture.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Small.Local.Path));
            return;
        }

        var file = await _client.ExecuteAsync(new TdApi.DownloadFile
        {
            FileId = chat.Photo.Small.Id,
            Priority = 1
        });

        if (file.Local.IsDownloadingCompleted)
        { 
            avatar._personPicture.ProfilePicture = new BitmapImage(new Uri(file.Local.Path));
        }
        else if (chat.Photo.Small.Local.IsDownloadingCompleted)
        {
            avatar._personPicture.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Small.Local.Path));
        }
    }
    
    public static async Task GetUserPhoto(TdApi.User user, ProfilePhoto avatar)
    {
        if (user.ProfilePhoto == null)
        {
            avatar._personPicture.DisplayName = user.FirstName + " " + user.LastName;
            return;
        }

        if (user.ProfilePhoto.Small.Local.IsDownloadingCompleted)
        {
            avatar._personPicture.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Small.Local.Path));
            return;
        }

        var file = await _client.ExecuteAsync(new TdApi.DownloadFile
        {
            FileId = user.ProfilePhoto.Small.Id,
            Priority = 1
        });

        if (file.Local.IsDownloadingCompleted)
        {
            avatar._personPicture.ProfilePicture = new BitmapImage(new Uri(file.Local.Path));
        }
        else if (user.ProfilePhoto.Small.Local.IsDownloadingCompleted)
        {
            avatar._personPicture.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Small.Local.Path));
        }
    }
}