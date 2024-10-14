using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Octokit;
using FileMode = Octokit.FileMode;

namespace sakuragram.Services;

public class UpdateManager
{
    private readonly HttpClient _httpClient;
    private static readonly GitHubClient _gitHubClient = App._githubClient;
    
    public string _newVersion;

    public UpdateManager()
    {
        _httpClient = new HttpClient();
    }
    
    public async Task<string> GetLatestReleaseFromGitHub()
    {
        var releases = await _gitHubClient.Repository.Release.GetAll(Config.GitHubRepoOwner, Config.GitHubRepoName).ConfigureAwait(false);
        Release latestRelease = releases[0];
        
        return latestRelease.TagName;
    }
    
    public async Task<bool> CheckForUpdates()
    {
        var newVersion = await GetLatestReleaseFromGitHub();
        var currentVersion = Config.AppVersion;

        if (newVersion != null && newVersion != currentVersion)
        {
            newVersion = newVersion.Replace(".", "");
            currentVersion = currentVersion.Replace(".", "");

            if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
            {
                _newVersion = newVersion;
                return true;
            }
            else
            {
                _newVersion = currentVersion;
                return false;
            }
        }
        else
        {
            return false; 
        }
    }

    public async Task UpdateApplicationAsync()
    {
        string updateLink = _gitHubClient.Repository.Release.GetLatest(Config.GitHubRepoOwner, Config.GitHubRepoName)
            .Result.Assets[1].BrowserDownloadUrl;
    
        var latestVersion = await GetLatestReleaseFromGitHub();
        var currentVersion = Config.AppVersion;

        if (latestVersion != currentVersion)
        {
            var updateResponse = await _httpClient.GetAsync(updateLink);
            updateResponse.EnsureSuccessStatusCode();

            var updateFilePath = Path.Combine(Path.GetTempPath(), $"{Config.AppName}_{latestVersion}.msi");
            await using (var fileStream = new FileStream(updateFilePath, System.IO.FileMode.Create))
            {
                await updateResponse.Content.CopyToAsync(fileStream);
            }

            ApplyUpdate(updateFilePath);
        }
    }
    
    public static void ApplyUpdate(string file)
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