using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TdLib;

namespace sakuragram.Services.Core;

public class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "sakuragram.settings.json");
    private static readonly string AccountSettingsPath = Path.Combine(AppContext.BaseDirectory, @"tdata\sakuragram.accounts.json");
    private static string AccountInfoPath = Path.Combine(AppContext.BaseDirectory, 
        @$"tdata\u_{AccountManager.GetSelectedAccount().Result}\sakuragram.account_info.json");

    public static void UpdateAccountInfoPath()
    {
        AccountInfoPath = Path.Combine(AppContext.BaseDirectory, 
            @$"tdata\u_{AccountManager.GetSelectedAccount().Result}\sakuragram.account_info.json");
    }
    
    public static void SaveSettings(Settings settings)
    {
        string jsonSettings = JsonConvert.SerializeObject(settings);
        File.WriteAllText(SettingsPath, jsonSettings);
    }
    
    public static Settings LoadSettings()
    {
        if (!File.Exists(SettingsPath)) return new Settings
        {
            AutoUpdate = true, 
            InstallBeta = false,
            Language = "en",
            ChatBottomFastAction = "Discuss",
            StartMediaPage = "Stickers",
            ArchiveState = true
        };
        string jsonSettings = File.ReadAllText(SettingsPath);
        return JsonConvert.DeserializeObject<Settings>(jsonSettings);
    }
    
    public static void SaveAccountSettings(AccountSettings settings)
    {
        string jsonSettings = JsonConvert.SerializeObject(settings);
        File.WriteAllText(AccountSettingsPath, jsonSettings);
    }
    
    public static AccountSettings LoadAccountSettings()
    {
        if (!File.Exists(AccountSettingsPath)) return new AccountSettings
        {
            SelectedAccount = 0,
            AccountIds = new List<long>()
        };
        string jsonSettings = File.ReadAllText(AccountSettingsPath);
        return JsonConvert.DeserializeObject<AccountSettings>(jsonSettings);
    }

    public static void SaveAccountInfoSettings(AccountInfo settings)
    {
        string jsonSettings = JsonConvert.SerializeObject(settings);
        File.WriteAllText(AccountInfoPath, jsonSettings);
    }
    
    public static AccountInfo LoadAccountInfoSettings()
    {
        if (!File.Exists(AccountInfoPath)) return new AccountInfo
        {
            ChatFolders = []
        };
        string jsonSettings = File.ReadAllText(AccountInfoPath);
        return JsonConvert.DeserializeObject<AccountInfo>(jsonSettings);
    }
}

public class Settings
{
    public bool InstallBeta { get; set; }
    public bool AutoUpdate { get; set; }
    public string Language { get; set; }
    public string ChatBottomFastAction { get; set; }
    public string StartMediaPage { get; set; }
    public bool ArchiveState { get; set; }
}

public class AccountSettings
{ 
    public int SelectedAccount { get; set; }
    public List<long> AccountIds { get; set; }
}

public class AccountInfo
{ 
    public TdApi.ChatFolderInfo[] ChatFolders { get; set; }
}