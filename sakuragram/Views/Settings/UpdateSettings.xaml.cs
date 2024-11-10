using Windows.Storage;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using sakuragram.Services.Core;

namespace sakuragram.Views.Settings;

public partial class UpdateSettings : Page
{
    private static Services.Core.Settings _localSettings;
    private string _appName;
    private readonly string _appLatestVersion;
    private string _appLatestVersionLink;
    
    public UpdateSettings()
    {
        InitializeComponent();

        DispatcherQueue.EnqueueAsync(() =>
        {
            _localSettings = SettingsService.LoadSettings();
            
            ToggleSwitchInstallBeta.IsOn = _localSettings.InstallBeta;
            ToggleSwitchAutoUpdate.IsOn = _localSettings.AutoUpdate;
        });
    }
    
    #region setting parameters

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        SettingsService.SaveSettings(new Services.Core.Settings
        {
            InstallBeta = ToggleSwitchInstallBeta.IsOn,
            AutoUpdate = ToggleSwitchAutoUpdate.IsOn
        });
    }

    private void ToggleSwitchAutoUpdate_OnToggled(object sender, RoutedEventArgs e)
    {
        SettingsService.SaveSettings(new Services.Core.Settings
        {
            InstallBeta = ToggleSwitchInstallBeta.IsOn,
            AutoUpdate = ToggleSwitchAutoUpdate.IsOn
        });
    }

    #endregion
}