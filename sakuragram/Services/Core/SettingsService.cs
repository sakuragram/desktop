using System;
using System.IO;
using Newtonsoft.Json;

namespace sakuragram.Services.Core;

public class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "sakura_settings.json");
    
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
            StartMediaPage = "Stickers"
        };
        string jsonSettings = File.ReadAllText(SettingsPath);
        return JsonConvert.DeserializeObject<Settings>(jsonSettings);
    }
}

public class Settings
{
    public bool InstallBeta { get; set; }
    public bool AutoUpdate { get; set; }
    public string Language { get; set; }
    public string ChatBottomFastAction { get; set; }
    public string StartMediaPage { get; set; }
}