using System.Text.RegularExpressions;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using sakuragram.Controls.User;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Controls.Messages;

public class SearchEntry : StackPanel
{
    public long ChatId;
    
    public SearchEntry(TdApi.Chat chat, TdApi.User user, TdApi.Message message)
    {
        if (chat != null) ChatId = chat.Id;
        
        DispatcherQueue.EnqueueAsync(async () =>
        {
            Orientation = Orientation.Horizontal;
            
            ProfilePhoto avatar = new ProfilePhoto();
            if (user != null)
                await avatar.InitializeProfilePhoto(user, null);
            else if (chat != null)
                await avatar.InitializeProfilePhoto(null, chat);
            avatar.Margin = new Thickness(0, 0, 4, 0);
            Children.Add(avatar);
            
            StackPanel panel = new StackPanel();
            TextBlock chatTitle = new TextBlock();
            chatTitle.FontSize = 14;
            chatTitle.Text = chat != null 
                ? chat.Title 
                : user != null 
                    ? string.IsNullOrEmpty(user.LastName) 
                        ? user.FirstName 
                        : $"{user.FirstName} {user.LastName}"
                    : "";
            panel.Children.Add(chatTitle);

            if (message != null)
            {
                var messageContentText = await MessageService.GetTextMessageContent(message);
                messageContentText = Regex.Replace(messageContentText, @"\s+", " ");
                messageContentText = messageContentText.Trim();
                
                TextBlock messageText = new TextBlock();
                messageText.Text = messageContentText;
                messageText.FontSize = 12;
                messageText.Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"];
                
                panel.Children.Add(messageText);
            }
            Children.Add(panel);
        });
    }
}