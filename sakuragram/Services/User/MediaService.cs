using System;
using System.Threading;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;

namespace sakuragram.Services;

public class MediaService
{
    private static TdClient _client = App._client;
    
    public static async void GetChatPhoto(TdApi.Chat chat, PersonPicture avatar)
    {
        if (chat.Photo == null)
        {
            avatar.DisplayName = chat.Title;
            return;
        }

        if (chat.Photo.Small.Local.Path != string.Empty)
        {
            avatar.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Small.Local.Path));
        }
        else
        {
            var file = await _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = chat.Photo.Small.Id,
                Priority = 1
            }).WaitAsync(new CancellationToken());
            if (file.Local.IsDownloadingCompleted) 
                avatar.ProfilePicture = new BitmapImage(new Uri(file.Local.Path != string.Empty ? file.Local.Path : chat.Photo.Small.Local.Path));
        }
    }
    
    public static async void GetUserPhoto(TdApi.User user, PersonPicture avatar)
    {
        if (user.ProfilePhoto == null)
        {
            avatar.DisplayName = user.FirstName + " " + user.LastName;
            return;
        }
        
        if (user.ProfilePhoto.Small.Local.Path != string.Empty)
        {
            avatar.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Small.Local.Path));
        }
        else
        {
            var file = await _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = user.ProfilePhoto.Small.Id,
                Priority = 1
            }).WaitAsync(new CancellationToken());
            if (file.Local.IsDownloadingCompleted) 
                avatar.ProfilePicture = new BitmapImage(new Uri(file.Local.Path != string.Empty ? file.Local.Path : user.ProfilePhoto.Small.Local.Path));
        }
    }
}