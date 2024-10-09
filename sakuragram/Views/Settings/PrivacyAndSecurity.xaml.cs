using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using sakuragram.Views.Settings.AdditionalElements;
using TdLib;

namespace sakuragram.Views.Settings;

public partial class PrivacyAndSecurity : Page
{
    private TdClient _client = App._client;
    
    public PrivacyAndSecurity()
    {
        InitializeComponent();
        UpdateInfo();
    }

    private async void UpdateInfo()
    {
        var blockedUsers = await _client.ExecuteAsync(new TdApi.GetBlockedMessageSenders
        {
            BlockList = new TdApi.BlockList.BlockListMain(), Limit = 100
        });
        var connectedWebsites = await _client.ExecuteAsync(new TdApi.GetConnectedWebsites());
        var activeSessions = await _client.ExecuteAsync(new TdApi.GetActiveSessions());
        
        CardBlockedUsers.Description = $"There are currently {blockedUsers.TotalCount} blocked users";
        CardConnectedWebsites.Description = $"There are currently {connectedWebsites.Websites.Length} connected websites";
        CardActiveSessions.Description = $"There are currently {activeSessions.Sessions_.Length} active sessions";
    }
    
    private void DeleteIfAway_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void ClearPaymentInfo_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private async void ContentDialogSessions_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        var sessions = await _client.GetActiveSessionsAsync();

        foreach (var session in sessions.Sessions_)
        {
            var sessionEntry = new Session();
            sessionEntry.Update(session);

            if (session.IsCurrent)
            {
                ActiveSession.Children.Add(sessionEntry);
            }
            else
            {
                SessionList.Children.Add(sessionEntry);
            }
        }
    }
    
    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        await ContentDialogSessions.ShowAsync();
    }

    private async void ButtonTerminateOtherSessions_OnClick(object sender, RoutedEventArgs e)
    {
        ContentDialogSessions.Hide();
        await ContentDialogSessions.ShowAsync();
    }

    private void TerminatingContentDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _client.TerminateAllOtherSessionsAsync();
        CardActiveSessions.Description = "There are currently 1 active sessions";
    }

    private void ContentDialogSessions_OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        ActiveSession.Children.Clear();
        SessionList.Children.Clear();
    }
}