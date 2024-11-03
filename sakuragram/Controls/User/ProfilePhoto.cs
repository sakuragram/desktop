using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Controls.User;

public class ProfilePhoto : PersonPicture
{
    public async Task SetPhoto(TdApi.User user, TdApi.Chat chat)
    {
        if (user != null)
        {
            await DispatcherQueue.EnqueueAsync(() => MediaService.GetUserPhoto(user, this));
        }
        else
        {
            await DispatcherQueue.EnqueueAsync(() => MediaService.GetChatPhoto(chat, this));
        }
    }
}