using Windows.UI;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using sakuragram.Services;
using sakuragram.Views.MediaPlayer;
using TdLib;

namespace sakuragram.Controls.User;

[Windows.UI.Xaml.Data.Bindable]
public class Story : Button
{
    private TdClient _client = App._client;
    
    public Story(TdApi.ChatActiveStories stories)
    {
        DispatcherQueue.EnqueueAsync(async () =>
        {
            Width = 32;
            Height = 32;
            HorizontalContentAlignment = HorizontalAlignment.Center;
            VerticalContentAlignment = VerticalAlignment.Center;
            
            ProfilePhoto avatar = null;
            
            if (stories.ChatId > 0)
            {
                var user = await _client.GetUserAsync(stories.ChatId);
                avatar = new ProfilePhoto();
                await avatar.InitializeProfilePhoto(user, sizes: 30);
            }
            else if (stories.ChatId < 0)
            {
                var chat = await _client.GetChatAsync(stories.ChatId);
                avatar = new ProfilePhoto();
                await avatar.InitializeProfilePhoto(chat: chat, sizes: 30);
            }

            Padding = new Thickness(0);
            Margin = new Thickness(0);
            CornerRadius = new CornerRadius(15);
            Background = new SolidColorBrush(Color.FromArgb(255,13,194,0));
            Style = null;
            Content = avatar;
            Click += (_, _) =>
            {
                var mediaPlayer = new MediaPlayer(stories);
                mediaPlayer.Activate();
            };
        });
    }
}