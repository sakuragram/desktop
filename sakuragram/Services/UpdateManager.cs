using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Octokit;

namespace sakuragram.Services;

public class UpdateManager
{
    private static readonly GitHubClient _gitHubClient = App._githubClient;
    public string _newVersion;
    public AsyncCompletedEventHandler _asyncCompletedEventHandler;

    private static async Task<string> GetLatestReleaseFromGitHub()
    {
        var releases = await _gitHubClient.Repository.Release.GetAll(Config.GitHubRepoOwner, Config.GitHubRepoName).ConfigureAwait(false);
        Release latestRelease = releases[0];
        
        return latestRelease.TagName;
    }
    
    [Obsolete("Obsolete")]
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

    private void InitScript()
    {
        string path = AppContext.BaseDirectory + @"\installUpdate.bat";
        
        Process process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = "";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.Verb = "runas";
        process.Start();
        Environment.Exit(1);
    }

    [Obsolete("Obsolete")]
    public void DownloadUpdate()
    {
        WebClient client = new WebClient();
        string path = AppContext.BaseDirectory + @"\sakuragram_Release_x64.msi"; 
        client.DownloadFileCompleted += _asyncCompletedEventHandler;
        client.DownloadFileAsync(new Uri(Config.LinkForUpdate), path);
    }

    public void Update()
    {
        InitScript();
    }
}