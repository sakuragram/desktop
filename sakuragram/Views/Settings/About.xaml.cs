using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Octokit;
using Page = Microsoft.UI.Xaml.Controls.Page;

namespace sakuragram.Views.Settings;

public partial class About : Page
{
    private GitHubClient _githubClient = App._githubClient;
    
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
    private string _appName = Config.AppName;
    private string _appLatestVersionLink;
    private static readonly Services.UpdateManager _updateManager = App.UpdateManager;
    
    public About()
    {
        InitializeComponent();
        
        _appLatestVersionLink = $"https://github.com/{Config.GitHubRepoOwner}/{Config.GitHubRepoName}/releases/tag/{Config.AppVersion}";
        
        TextBlockVersionInfo.Text = $"Current version: {Config.AppVersion}, TdLib {Config.TdLibVersion}";
        
        _updateManager._asyncCompletedEventHandler += AsyncCompletedEventHandler;
        CheckForUpdates();
        
        if (File.Exists(AppContext.BaseDirectory + @"\sakuragram_Release_x64.msi"))
        {
            ButtonNewVersionAvailable.Content = "Install";
            ButtonNewVersionAvailable.IsEnabled = true;
            ButtonNewVersionAvailable.Click += ButtonNewVersionAvailable_OnUpdateDownloaded;
        }
    }
    
    private void AsyncCompletedEventHandler(object sender, AsyncCompletedEventArgs e)
    {
        if (_localSettings.Values["AutoUpdate"] != null && (bool)_localSettings.Values["AutoUpdate"])
        {
            ButtonNewVersionAvailable.Content = "Installing...";
            _updateManager.Update();
        }
        else
        {
            ButtonNewVersionAvailable.Content = "Install";
            ButtonNewVersionAvailable.IsEnabled = true;
            ButtonNewVersionAvailable.Click += ButtonNewVersionAvailable_OnUpdateDownloaded;
        }
    }

    private void ButtonCheckForUpdates_OnClick(object sender, RoutedEventArgs e)
    {
        CheckForUpdates();
    }
    
    private void ButtonNewVersionAvailable_OnUpdateDownloaded(object sender, RoutedEventArgs e)
    {
        _updateManager.Update();
    }

    private async void CheckForUpdates()
    {
        try
        {
            ButtonCheckForUpdates.IsEnabled = false;
            CardCheckForUpdates.Description = "Checking for updates...";
            
            if (await _updateManager.CheckForUpdates())
            {
                CardCheckForUpdates.Description = $"New version available: {ThisAssembly.Git.BaseTag}";
                CardNewVersionAvailable.Visibility = Visibility.Visible;
            }
            else
            {
                CardCheckForUpdates.Description = "No updates found";
            }
            
            ButtonCheckForUpdates.IsEnabled = true;
        }
        catch (Exception e)
        {
            CardCheckForUpdates.Description = $"Error: {e.Message}";
            throw;
        }
    }
    
    private void ButtonNewVersionAvailable_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _updateManager.DownloadUpdate();
            ButtonNewVersionAvailable.Content = "Downloading...";
            ButtonNewVersionAvailable.IsEnabled = false;
        }
        catch (Exception exception)
        {
            CardNewVersionAvailable.Description = $"Error: {exception.Message}";
            ButtonNewVersionAvailable.Content = "Download failed";
            throw;
        }
    }

    private async void ExpanderReleases_OnLoaded(object sender, RoutedEventArgs e)
    {
        var releases = await _githubClient.Repository.Release.GetAll(Config.GitHubRepoOwner, Config.GitHubRepoName)
            .ConfigureAwait(false);
        
        await DispatcherQueue.EnqueueAsync(() =>
        {
            foreach (var release in releases)
            {
                string releaseName = release.Prerelease ? "Pre-release " + release.Name : "Release " + release.Name;
                string releaseBody = release.Body != string.Empty ? release.Body : "The release does not have a changelog";

                SettingsCard card = new();
                card.Header = releaseName;
                card.Description = releaseBody;

                ExpanderReleases.Items.Add(card);
            }
        });
    }
}