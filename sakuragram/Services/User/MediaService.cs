using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace sakuragram.Services;

public class MediaService
{
    private static TdClient _client = App._client;
    
    public static async Task GetChatPhoto(TdApi.Chat chat, PersonPicture avatar)
    {
        if (chat.Photo == null)
        {
            avatar.DisplayName = chat.Title;
            return;
        }

        if (chat.Photo.Small.Local.Path != null)
        {
            avatar.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Small.Local.Path));
            return;
        }

        var file = await _client.ExecuteAsync(new TdApi.DownloadFile
        {
            FileId = chat.Photo.Small.Id,
            Priority = 1
        });

        if (file.Local.IsDownloadingCompleted)
        {
            var path = file.Local.Path ?? chat.Photo.Small.Local.Path;
            avatar.ProfilePicture = new BitmapImage(new Uri(path));
        }
    }
    
    public static async Task GetUserPhoto(TdApi.User user, PersonPicture avatar)
    {
        if (user.ProfilePhoto == null)
        {
            avatar.DisplayName = user.FirstName + " " + user.LastName;
            return;
        }

        if (user.ProfilePhoto.Small.Local.Path != null)
        {
            avatar.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Small.Local.Path));
            return;
        }

        var file = await _client.ExecuteAsync(new TdApi.DownloadFile
        {
            FileId = user.ProfilePhoto.Small.Id,
            Priority = 1
        });

        if (file.Local.IsDownloadingCompleted)
        {
            var path = file.Local.Path ?? user.ProfilePhoto.Small.Local.Path;
            avatar.ProfilePicture = new BitmapImage(new Uri(path));
        }
    }
}