using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using sakuragram.Services;
using sakuragram.Services.Core;
using sakuragram.Views.MediaPlayers;
using TdLib;

namespace sakuragram.Controls.User;

public class ProfilePhoto : RelativePanel
{
    public PersonPicture _personPicture;
    private Border _statusBorder;
    
    private TdClient _client = App._client;
    private TdApi.User _user;
    private long _userId;
    private TdApi.Chat _chat;
    private TdApi.ChatActiveStories _stories;
    
    public async Task InitializeProfilePhoto(TdApi.User user = null, TdApi.Chat chat = null, int sizes = 32, 
        bool canOpenProfile = false)
    {
        if (user != null)
        {
            _user = user;
            _userId = user.Id;
        }
        
        if (chat != null) _chat = chat;
        
        await DispatcherQueue.EnqueueAsync(() =>
        {
            _personPicture = new PersonPicture();
            _personPicture.BorderThickness = new Thickness(10);
            _personPicture.Width = sizes;
            _personPicture.Height = sizes;
            
            _statusBorder = new Border();
            _statusBorder.Background = new SolidColorBrush(Colors.Cyan);
            _statusBorder.Width = 10;
            _statusBorder.Height = 10;
            _statusBorder.CornerRadius = new CornerRadius(5);
            _statusBorder.HorizontalAlignment = HorizontalAlignment.Right;
            _statusBorder.VerticalAlignment = VerticalAlignment.Bottom;
            SetAlignBottomWithPanel(_statusBorder, true);
            SetAlignRightWithPanel(_statusBorder, true);
            _statusBorder.Visibility = Visibility.Collapsed;
            
            Children.Add(_personPicture); 
            Children.Add(_statusBorder);
        });
        
        if (user != null) 
            await DispatcherQueue.EnqueueAsync(async () => await MediaService.GetUserPhoto(user, this));
        else if (chat != null)
            await DispatcherQueue.EnqueueAsync(async () => await MediaService.GetChatPhoto(chat, this));
        
        var stories = App._stories;
        if (stories != null)
        {
            foreach (var story in stories)
            {
                _stories = story;
                if (user != null)
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        if (story.ChatId != user.Id) return;
                        _personPicture.BorderThickness = new Thickness(10);
                        Logger.Log(Logger.LogType.Info, _personPicture.BorderThickness);
                        _personPicture.BorderBrush = new SolidColorBrush(Colors.GreenYellow);
                        _personPicture.PointerPressed += OpenStory;
                    });
                }
                else if (chat != null)
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        if (story.ChatId != chat.Id) return;
                        _personPicture.BorderThickness = new Thickness(10);
                        Logger.Log(Logger.LogType.Info, _personPicture.BorderThickness);
                        _personPicture.BorderBrush = new SolidColorBrush(Colors.GreenYellow);
                        _personPicture.PointerPressed += OpenStory;
                    });
                }
            }
            
            void OpenStory(object sender, PointerRoutedEventArgs e)
            {
                var mediaPlayer = new MediaPlayer(_stories);
            }
        }

        PointerPressed += async (_, _) =>
        {
            if (canOpenProfile) await UserService.ShowProfile(_user, _chat, XamlRoot);
        };
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateUserStatus updateUserStatus:
            {
                if (updateUserStatus.UserId == _userId)
                {
                    switch (updateUserStatus.Status)
                    {
                        case TdApi.UserStatus.UserStatusOnline:
                            await DispatcherQueue.EnqueueAsync(() => _statusBorder.Visibility = Visibility.Visible);
                            break;
                        case TdApi.UserStatus.UserStatusOffline or TdApi.UserStatus.UserStatusRecently 
                            or TdApi.UserStatus.UserStatusEmpty:
                            await DispatcherQueue.EnqueueAsync(() => _statusBorder.Visibility = Visibility.Collapsed);
                            break;
                    }
                }
                break;
            }
        }
    }
}