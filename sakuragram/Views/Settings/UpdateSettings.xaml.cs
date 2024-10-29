using Windows.Storage;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Views.Settings;

public partial class UpdateSettings : Page
{
    private static ApplicationDataContainer _localSettings;
    private string _appName;
    private readonly string _appLatestVersion;
    private string _appLatestVersionLink;
    
    public UpdateSettings()
    {
        InitializeComponent();

        DispatcherQueue.EnqueueAsync(() =>
        {
            if (ApplicationData.Current.LocalSettings.Containers.ContainsKey(Config.AppName))
            {
                _localSettings = ApplicationData.Current.LocalSettings.Containers[Config.AppName];
            }
            else
            {
                _localSettings = ApplicationData.Current.LocalSettings.CreateContainer(Config.AppName, ApplicationDataCreateDisposition.Always);
            }
            
            #region Settings

            if (_localSettings != null)
            {
                if (_localSettings.Values["AutoUpdate"] == null)
                {
                    ToggleSwitchAutoUpdate.IsOn = true;
                    _localSettings.Values["AutoUpdate"] = true;
                }
                else
                {
                    var autoUpdateValue = _localSettings.Values["AutoUpdate"];
                    ToggleSwitchAutoUpdate.IsOn = (bool)autoUpdateValue;
                }
            
                if (_localSettings.Values["InstallBeta"] == null)
                {
                    ToggleSwitchInstallBeta.IsOn = false;
                    _localSettings.Values["InstallBeta"] = false;
                }
                else
                {
                    var installBetaValue = _localSettings.Values["InstallBeta"];
                    ToggleSwitchInstallBeta.IsOn = (bool)installBetaValue;
                }
            }

            #endregion
        });
    }
    
    #region setting parameters

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        _localSettings.Values["InstallBeta"] = ToggleSwitchInstallBeta.IsOn;
    }

    private void ToggleSwitchAutoUpdate_OnToggled(object sender, RoutedEventArgs e)
    {
        _localSettings.Values["AutoUpdate"] = ToggleSwitchAutoUpdate.IsOn;
    }

    #endregion
}