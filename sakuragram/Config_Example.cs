namespace sakuragram;

/** Rename file and class to Config */
public class Config_Example
{
    public const string AppName = "your_app_name";
    public static readonly string AppVersion = GetAppVersion();
    
    public const string BaseLocation = "tdata";
    
    /** You can get your Telegram API keys on the website my.telegram.org. I recommend updating the variable with its version every time TDLib is updated */
    public const int ApiId = 0;
    public const string ApiHash = "your_api_hash";
    public const string TdLibVersion = "1.8.37";
    
    /** You need to create a GitHub Api Token for GitHub-related functions (in particular UpdateManager) to work correctly */
    public const string GitHubAuthToken = "your_github_auth_token";
    public const string GitHubRepoOwner = "your_github_repo_owner";
    public const string GitHubRepoName = "your_github_repo_name";
    
    private static string GetAppVersion() {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion;
    }
}