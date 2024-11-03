﻿using System;
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

        if (chat.Photo.Small.Local.Path != string.Empty)
        {
            await avatar.DispatcherQueue.EnqueueAsync(() =>
                avatar.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Small.Local.Path)));
        }
        else
        {
            var file = await _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = chat.Photo.Small.Id,
                Priority = 1
            }).ConfigureAwait(false);
            if (file.Local.IsDownloadingCompleted) 
                await avatar.DispatcherQueue.EnqueueAsync(() => 
                    avatar.ProfilePicture = new BitmapImage(new Uri(file.Local.Path != string.Empty 
                        ? file.Local.Path 
                        : chat.Photo.Small.Local.Path)));
        }
    }
    
    public static async Task GetUserPhoto(TdApi.User user, PersonPicture avatar)
    {
        if (user.ProfilePhoto == null)
        {
            avatar.DisplayName = user.FirstName + " " + user.LastName;
            return;
        }
        
        if (user.ProfilePhoto.Small.Local.Path != string.Empty)
        {
            await avatar.DispatcherQueue.EnqueueAsync(() =>
                avatar.ProfilePicture = new BitmapImage(new Uri(user.ProfilePhoto.Small.Local.Path)));
        }
        else
        {
            var file = await _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = user.ProfilePhoto.Small.Id,
                Priority = 1
            }).ConfigureAwait(false);
            if (file.Local.IsDownloadingCompleted)
                await avatar.DispatcherQueue.EnqueueAsync(() =>
                    avatar.ProfilePicture = new BitmapImage(new Uri(file.Local.Path != string.Empty
                        ? file.Local.Path
                        : user.ProfilePhoto.Small.Local.Path)));
        }
    }
}