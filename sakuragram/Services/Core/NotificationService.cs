using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Windows.AppNotifications.Builder;
using TdLib;

namespace sakuragram.Services.Core;

public class NotificationService
{
    private static TdClient _client = App._client;

    public NotificationService()
    {
        //_client.UpdateReceived += async (_, update) => { await ProcessUpdate(update); };
    }

    private async Task ProcessUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateNewMessage updateNewMessage:
            {
                SendNotification(updateNewMessage.Message);
                break;
            }
        }
    }

    private async void SendNotification(TdApi.Message message)
    {
        if (message is null || _client is null) return;
        try
        {
            TdApi.Chat chat = await _client.ExecuteAsync(new TdApi.GetChat {ChatId = message.ChatId});
            var sender = await UserService.GetSender(message.SenderId).ConfigureAwait(false);
            var senderName = await UserService.GetSenderName(message).ConfigureAwait(false);
            
            if (sender.User != null && sender.User.Id == _client.GetMeAsync().Result.Id) return;
            if (chat.NotificationSettings.MuteFor > 100000000) return;
            
            new ToastContentBuilder()
                .AddArgument("conversationId", chat.Id)
                .AddText($"{senderName} sent you new message!")
                .AddText(MessageService.GetTextMessageContent(message).Result)
                .AddButton(new ToastButton()
                    .SetContent("Reply"))
                .AddArgument("action", "reply")
                .AddButton(new ToastButton()
                    .SetContent("Mark as read")
                    .AddArgument("action", "markasread"))
                .Show();
        }
        catch (TdException e)
        {
            Debug.WriteLine(e.Message);
            throw;
        }
    }
}