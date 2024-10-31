using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Octokit;
using sakuragram.Services;
using Page = Microsoft.UI.Xaml.Controls.Page;

namespace sakuragram.Views.Settings;

public partial class About : Page
{
    private GitHubClient _githubClient = App._githubClient;
    private ApplicationDataContainer _localSettings = MainWindow._localSettings;
    private string _appName = Config.AppName;
    private string _appLatestVersionLink;
    
    private static readonly UpdateManager _updateManager = App.UpdateManager;
    
    public About()
    {
        InitializeComponent();
        
        _appLatestVersionLink = $"https://github.com/{Config.GitHubRepoOwner}/{Config.GitHubRepoName}/releases/tag/v{Config.AppVersion}";
        TextBlockVersionInfo.Text = $"Current version: {Config.AppVersion}, TdLib {Config.TdLibVersion}";
        
        CheckForUpdates();
    }

    private void ButtonCheckForUpdates_OnClick(object sender, RoutedEventArgs e)
    {
        CheckForUpdates();
    }

    private async void CheckForUpdates()
    {
        try
        {
            ButtonCheckForUpdates.IsEnabled = false;
            CardNewVersionAvailable.Visibility = Visibility.Collapsed;
            CardCheckForUpdates.Description = "Checking for updates...";
            
            if (await _updateManager.CheckForUpdates())
            {
                CardCheckForUpdates.Description = $"New version available: {_updateManager.GetLatestReleaseFromGitHub().Result}";
                CardNewVersionAvailable.Visibility = Visibility.Visible;
                ButtonNewVersionAvailable.Click += ButtonNewVersionAvailable_OnClick;
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

    private async void ButtonNewVersionAvailable_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            ButtonNewVersionAvailable.Content = "Downloading...";
            ButtonNewVersionAvailable.IsEnabled = false;
            string updateFilePath = await _updateManager.UpdateApplicationAsync();
            
            if (updateFilePath != string.Empty)
            {
                ButtonNewVersionAvailable.Content = "Install";
                ButtonNewVersionAvailable.IsEnabled = true;
                ButtonNewVersionAvailable.Click += (o, args) =>
                {
                    ButtonNewVersionAvailable.Content = "Installing...";
                    ButtonNewVersionAvailable.IsEnabled = false;
                    _updateManager.ApplyUpdate(updateFilePath);
                };
            }
        }
        catch (Exception exception)
        {
            CardNewVersionAvailable.Description = $"Error: {exception.Message}";
            ButtonNewVersionAvailable.Content = "Download failed";
            ButtonNewVersionAvailable.IsEnabled = true;
            throw;
        }
    }

    private async void ExpanderReleases_OnLoaded(object sender, RoutedEventArgs e)
    {
        var releases = await _githubClient.Repository.Release.GetAll(Config.GitHubRepoOwner, Config.GitHubRepoName)
            .ConfigureAwait(false);
        
        await DispatcherQueue.EnqueueAsync(() =>
        {
            if (releases.Count <= 0)
            {
                SettingsCard card = new();
                card.Header = "Not found";

                ExpanderReleases.Items.Add(card);
            }
            else
            {
                foreach (var release in releases)
                {
                    if (release.PublishedAt != null)
                    {
                        string releaseName = release.Prerelease ? "Pre-release " + release.Name : "Release " + release.Name 
                            + ", " + release.PublishedAt.Value.ToString("MM/dd/yyyy");
                        string releaseBody = release.Body != string.Empty ? release.Body : "The release does not have a changelog";

                        SettingsCard card = new();
                        card.Header = releaseName;
                        card.Description = releaseBody;

                        ExpanderReleases.Items.Add(card);
                    }
                }
            }
        });
    }
}