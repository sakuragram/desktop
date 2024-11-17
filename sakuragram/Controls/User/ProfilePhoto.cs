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
using sakuragram.Views.MediaPlayer;
using TdLib;

namespace sakuragram.Controls.User;

public class ProfilePhoto : RelativePanel
{
    public PersonPicture _personPicture;
    private Border _statusBorder;
    
    private TdClient _client = App._client;
    private TdApi.User _user;
    private TdApi.Chat _chat;
    private TdApi.ChatActiveStories _stories;
    
    public async Task InitializeProfilePhoto(TdApi.User user, TdApi.Chat chat, int width = 32, int height = 32)
    {
        if (user != null) _user = user;
        if (chat != null) _chat = chat;

        await DispatcherQueue.EnqueueAsync(() => _personPicture = new PersonPicture());
        _personPicture.BorderThickness = new Thickness(10);
        
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
        
        if (user != null) await MediaService.GetUserPhoto(user, this);
        else if (chat != null) await MediaService.GetChatPhoto(chat, this);
        
        await DispatcherQueue.EnqueueAsync(() =>
        {
            _personPicture.Width = width;
            _personPicture.Height = height;
            Children.Add(_personPicture); 
        });

        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateUserStatus updateUserStatus:
            {
                if (_user != null && updateUserStatus.UserId == _user.Id)
                {
                    switch (updateUserStatus.Status)
                    {
                        case TdApi.UserStatus.UserStatusOnline:
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                _statusBorder = new Border();
                                _statusBorder.Background = new SolidColorBrush(Colors.Cyan);
                                _statusBorder.Width = 10;
                                _statusBorder.Height = 10;
                                _statusBorder.CornerRadius = new CornerRadius(5);
                                _statusBorder.HorizontalAlignment = HorizontalAlignment.Right;
                                _statusBorder.VerticalAlignment = VerticalAlignment.Bottom;
                                Children.Add(_statusBorder);
                            });
                            break;
                        case TdApi.UserStatus.UserStatusOffline:
                            await DispatcherQueue.EnqueueAsync(() =>
                            {
                                Children.Remove(_statusBorder);
                                _statusBorder = null;
                            });
                            break;
                        case TdApi.UserStatus.UserStatusRecently:
                            await DispatcherQueue.EnqueueAsync(() => 
                            {
                                Children.Remove(_statusBorder);
                                _statusBorder = null;
                            });
                            break;
                    }
                }
                break;
            }
        }
    }
}