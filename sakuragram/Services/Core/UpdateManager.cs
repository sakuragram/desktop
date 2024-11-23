using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Octokit;
using FileMode = Octokit.FileMode;

namespace sakuragram.Services.Core;

public class UpdateManager
{
    private readonly HttpClient _httpClient;
    private static readonly GitHubClient _gitHubClient = App._githubClient;
    
    public string _latestVersion;

    public UpdateManager()
    {
        _httpClient = new HttpClient();
    }
    
    public async Task<string> GetLatestReleaseFromGitHub()
    {
        var localSettings = SettingsService.LoadSettings();

        if (localSettings.InstallBeta)
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(Config.GitHubRepoOwner, Config.GitHubRepoName);
            Release latestRelease = releases[0];
        
            return latestRelease.TagName;
        }
        else
        {
            var releases = await _gitHubClient.Repository.Release.GetLatest(Config.GitHubRepoOwner, Config.GitHubRepoName);
            Release latestRelease = releases;
        
            return latestRelease.TagName;
        }
    }
    
    public async Task<bool> CheckForUpdates()
    {
        var latestVersion = await GetLatestReleaseFromGitHub();
        var currentVersion = Config.AppVersion;

        if (latestVersion != null && latestVersion != currentVersion)
        {
            if (latestVersion.Contains("v")) latestVersion = latestVersion.Replace("v", "");
            if (latestVersion.Contains("-beta")) latestVersion = latestVersion.Replace("-beta", "");
            latestVersion = latestVersion.Replace(".", "");
            currentVersion = currentVersion.Replace(".", "");

            if (Convert.ToInt32(latestVersion) > Convert.ToInt32(currentVersion))
            {
                _latestVersion = latestVersion;
                return true;
            }
            else
            {
                _latestVersion = currentVersion;
                return false;
            }
        }
        else
        {
            return false; 
        }
    }

    public async Task<string> UpdateApplicationAsync()
    {
        string updateLink = _gitHubClient.Repository.Release.GetLatest(Config.GitHubRepoOwner, Config.GitHubRepoName)
            .Result.Assets[0].BrowserDownloadUrl;

        if (_latestVersion != Config.AppVersion)
        {
            var updateResponse = await _httpClient.GetAsync(updateLink);
            updateResponse.EnsureSuccessStatusCode();

            var updateFilePath = Path.Combine(Path.GetTempPath(), $"{Config.AppName}_{_latestVersion}.msi");
            await using (var fileStream = new FileStream(updateFilePath, System.IO.FileMode.Create))
            {
                await updateResponse.Content.CopyToAsync(fileStream);
            }
            return updateFilePath;
        }
        else
        {
            return string.Empty;
        }
    }
    
    public void ApplyUpdate(string file)
    {
        Process process = new();
        process.StartInfo.FileName = "msiexec.exe";
        process.StartInfo.Arguments = $"/i {file} /quiet";
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "runas";
        process.Start();
        Environment.Exit(1);
    }
}