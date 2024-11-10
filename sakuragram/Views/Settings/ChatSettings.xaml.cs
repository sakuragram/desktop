using Microsoft.UI.Xaml.Controls;
using sakuragram.Services.Core;

namespace sakuragram.Views.Settings;

public partial class ChatSettings : Page
{
    private Services.Core.Settings _localSettings = SettingsService.LoadSettings();
    
    public ChatSettings()
    {
        InitializeComponent();

        ComboBoxChannelBottomButton.SelectedIndex = _localSettings.ChatBottomFastAction switch
        {
            "Discuss" => 0,
            "Mute/Unmute" => 1,
            "Hide" => 2,
            _ => 0
        };
        ComboBoxStartMediaPage.SelectedIndex = _localSettings.StartMediaPage switch
        {
            "Emojis" => 0,
            "Stickers" => 1,
            "GIFs" => 2,
            _ => 0
        };
    }

    private void ComboBoxChannelBottomButton_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var value = ComboBoxChannelBottomButton.SelectedValue as ComboBoxItem;
        SettingsService.SaveSettings(new Services.Core.Settings
        {
            InstallBeta = _localSettings.InstallBeta,
            AutoUpdate = _localSettings.AutoUpdate,
            Language = _localSettings.Language,
            ChatBottomFastAction = value?.Content.ToString(),
            StartMediaPage = _localSettings.StartMediaPage
        });
    }

    private void ComboBoxStartMediaPage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var value = ComboBoxStartMediaPage.SelectedValue as ComboBoxItem;
        SettingsService.SaveSettings(new Services.Core.Settings
        {
            InstallBeta = _localSettings.InstallBeta,
            AutoUpdate = _localSettings.AutoUpdate,
            Language = _localSettings.Language,
            ChatBottomFastAction = _localSettings.ChatBottomFastAction,
            StartMediaPage = value?.Content.ToString()
        });
    }
}