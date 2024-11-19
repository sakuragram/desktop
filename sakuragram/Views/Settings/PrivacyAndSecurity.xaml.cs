using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using sakuragram.Controls.User;
using sakuragram.Services;
using sakuragram.Services.Core;
using sakuragram.Views.Settings.AdditionalElements;
using TdLib;

namespace sakuragram.Views.Settings;

public partial class PrivacyAndSecurity : Page
{
    private static readonly TdClient _client = App._client;
    
    private bool _canOpenBlockedUsers = false;
    private bool _canOpenConnectedWebsites = false;
    private bool _canOpenActiveSessions = false;
    
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
        _canOpenBlockedUsers = true;
        CardConnectedWebsites.Description = $"There are currently {connectedWebsites.Websites.Length} connected websites";
        _canOpenConnectedWebsites = true;
        CardActiveSessions.Description = $"There are currently {activeSessions.Sessions_.Length} active sessions";
        _canOpenActiveSessions = true;
        
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingShowStatus(),
            null, TextBlockLastSeenAndOnline, ComboBoxLastSeen);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingShowProfilePhoto(),
            ExpanderProfilePhotos, TextBlockProfilePhotos, ComboBoxProfilePhotos);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingShowBirthdate(),
            ExpanderDateOfBirth, TextBlockDateOfBirth, ComboBoxDateOfBirth);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingShowLinkInForwardedMessages(),
            ExpanderForwardedMessages, TextBlockForwardedMessages, ComboBoxForwardedMessages);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingAllowCalls(),
            ExpanderCalls, TextBlockCalls, ComboBoxCalls);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingAllowChatInvites(),
            ExpanderGroupsAndChannels, TextBlockGroupsAndChannels, ComboBoxGroupsAndChannels);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingAllowPrivateVoiceAndVideoNoteMessages(),
            ExpanderVoiceMessages, TextBlockVoiceMessages, ComboBoxVoiceMessages);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingShowPhoneNumber(),
            ExpanderPhoneNumber, PhoneNumber, ComboBoxPhoneNumber);
        await GetSettingAndUpdateUi(new TdApi.UserPrivacySetting.UserPrivacySettingAllowFindingByPhoneNumber(),
            null, null, ComboBoxFindMeByNumber);
    }
    
    private async Task GetSettingAndUpdateUi(TdApi.UserPrivacySetting setting, SettingsExpander expander, 
        TextBlock textBlock, ComboBox comboBox)
    {
        var userPrivacySetting = await _client.GetUserPrivacySettingRulesAsync(setting);
        
        if (userPrivacySetting.Rules.Length > 0)
        {
            string rule = string.Empty;
            int restricts = 0;
            
            foreach (var userPrivacySettingRule in userPrivacySetting.Rules)
            {
                switch (userPrivacySettingRule)
                {
                    case TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowAll:
                        rule = "Everybody";
                        break;
                    case TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowContacts:
                        rule = "My contacts";
                        break;
                    case TdApi.UserPrivacySettingRule.UserPrivacySettingRuleRestrictAll:
                        rule = "Nobody";
                        break;
                    case TdApi.UserPrivacySettingRule.UserPrivacySettingRuleRestrictChatMembers restrictChatMembers:
                        restricts += restrictChatMembers.ChatIds.Length;
                        break;
                    case TdApi.UserPrivacySettingRule.UserPrivacySettingRuleRestrictUsers restrictUsers:
                        restricts += restrictUsers.UserIds.Length;
                        break;
                }
                
                if (comboBox != null) comboBox.SelectedIndex = userPrivacySettingRule switch
                {
                    TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowAll => 0,
                    TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowContacts => 1,
                    TdApi.UserPrivacySettingRule.UserPrivacySettingRuleRestrictAll => 2,
                    _ => 0
                };
            }
            
            if (textBlock != null) textBlock.Text = restricts > 0 ? $"{rule} (-{restricts})" : rule;
            if (comboBox != null) comboBox.SelectionChanged += (_, _) =>
            {
                comboBox.SelectionChanged += async (_, _) =>
                {
                    TdApi.UserPrivacySettingRule rule;
                    switch (comboBox.SelectedIndex)
                    {
                        case 0:
                            rule = new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowAll();
                            break;
                        case 1:
                            rule = new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowContacts();
                            break;
                        case 2:
                            rule = new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleRestrictAll();
                            break;
                        default:
                            rule = new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowAll();
                            break;
                    }
                    await _client.SetUserPrivacySettingRulesAsync(setting, new TdApi.UserPrivacySettingRules
                    {
                        Rules = [rule]
                    });
                };
            };
        }
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
    
    private async void ButtonSessions_OnClick(object sender, RoutedEventArgs e)
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

    private async void ContentDialogWebsites_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        var websites = await _client.ExecuteAsync(new TdApi.GetConnectedWebsites()).ConfigureAwait(false);

        await DispatcherQueue.EnqueueAsync(() =>
        {
            foreach (var website in websites.Websites)
            {
                GenerateWebsiteEntry(website);
            }
        });
    }

    private async void GenerateWebsiteEntry(TdApi.ConnectedWebsite website)
    {
        TdApi.User bot = await _client.GetUserAsync(website.BotUserId);
        
        Border border = new();
        border.Width = 350;
        border.HorizontalAlignment = HorizontalAlignment.Stretch;
        border.VerticalAlignment = VerticalAlignment.Stretch;
        
        RelativePanel relativePanel = new();
        relativePanel.HorizontalAlignment = HorizontalAlignment.Stretch;
        relativePanel.VerticalAlignment = VerticalAlignment.Stretch;
        
        StackPanel mainStackPanel = new();
        mainStackPanel.Orientation = Orientation.Horizontal;
        mainStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
        mainStackPanel.VerticalAlignment = VerticalAlignment.Stretch;
        
        StackPanel textStackPanel = new();
        textStackPanel.Orientation = Orientation.Vertical;
        textStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
        textStackPanel.VerticalAlignment = VerticalAlignment.Stretch;

        ProfilePhoto botPhoto = new();
        await botPhoto.InitializeProfilePhoto(bot, null);
        botPhoto.Margin = new Thickness(10);
        botPhoto.Width = 42;
        botPhoto.Height = 42;
        mainStackPanel.Children.Add(botPhoto);
        
        TextBlock botNameTextBlock = new();
        botNameTextBlock.FontSize = 14;
        botNameTextBlock.Text = bot.FirstName + " " + bot.LastName;
        botNameTextBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
        textStackPanel.Children.Add(botNameTextBlock);
        
        TextBlock websiteInfo = new();
        websiteInfo.FontSize = 12;
        websiteInfo.Text = $"{website.DomainName}, {website.Browser}, {website.Platform}";
        websiteInfo.HorizontalAlignment = HorizontalAlignment.Stretch;
        textStackPanel.Children.Add(websiteInfo);
        
        TextBlock websiteLocation = new();
        websiteLocation.FontSize = 12;
        websiteLocation.Foreground = new SolidColorBrush(Colors.Gray);
        websiteLocation.Text = website.Location + ", " + MathService.CalculateDateTime(website.LogInDate).ToShortDateString();
        websiteLocation.HorizontalAlignment = HorizontalAlignment.Stretch;
        textStackPanel.Children.Add(websiteLocation);

        Button disconnectButton = new();
        disconnectButton.Content = new FontIcon { Glyph = "\uE711" };
        disconnectButton.Click += (sender, args) => DisconnectButton_Click(sender, args, website.Id);
        
        mainStackPanel.Children.Add(textStackPanel);
        mainStackPanel.Children.Add(disconnectButton);
        relativePanel.Children.Add(mainStackPanel);
        border.Child = relativePanel;
        PanelWebsites.Children.Add(border);
    }

    private async void DisconnectButton_Click(object sender, RoutedEventArgs e, long websiteId)
    {
        await _client.DisconnectWebsiteAsync(websiteId);
        PanelWebsites.Children.Clear();
        ContentDialogWebsites_OnOpened(null, null);
    }

    private async void ButtonDisconnectAllWebsites_OnClick(object sender, RoutedEventArgs e)
    {
        await _client.DisconnectAllWebsitesAsync();
        PanelWebsites.Children.Clear();
        ContentDialogWebsites_OnOpened(null, null);
    }
    
    private void ContentDialogWebsites_OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        PanelWebsites.Children.Clear();
    }

    private void ButtonViewWebsites_OnClick(object sender, RoutedEventArgs e)
    {
        ContentDialogWebsites.ShowAsync();
    }

    private void ButtonBlockedUsers_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void ComboBoxPhoneNumber_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TdApi.UserPrivacySettingRule rule = ComboBoxPhoneNumber.SelectedIndex switch
        {
            0 => new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowAll(),
            1 => new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowContacts(),
            2 => new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleRestrictAll(),
            _ => null
        };

        CardFindMeByNumber.Visibility = ComboBoxPhoneNumber.SelectedIndex switch
        {
            0 => Visibility.Collapsed,
            1 => Visibility.Collapsed,
            2 => Visibility.Visible,
            _ => Visibility.Collapsed
        };
        
        _client.ExecuteAsync(new TdApi.SetUserPrivacySettingRules
        {
            Rules = new TdApi.UserPrivacySettingRules { Rules = [rule] },
            Setting = new TdApi.UserPrivacySetting.UserPrivacySettingShowPhoneNumber()
        });
    }

    private void ComboBoxFindMeByNumber_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TdApi.UserPrivacySettingRule rule = ComboBoxFindMeByNumber.SelectedIndex switch
        {
            0 => new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowAll(),
            1 => new TdApi.UserPrivacySettingRule.UserPrivacySettingRuleAllowContacts(),
            _ => null
        };

        _client.ExecuteAsync(new TdApi.SetUserPrivacySettingRules
        {
            Rules = new TdApi.UserPrivacySettingRules { Rules = [rule] },
            Setting = new TdApi.UserPrivacySetting.UserPrivacySettingAllowFindingByPhoneNumber(),
        });
    }
}