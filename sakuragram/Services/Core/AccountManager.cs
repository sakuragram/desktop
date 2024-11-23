using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using sakuragram.Views;
using TdLib;

namespace sakuragram.Services.Core;

public class AccountManager
{
    private TdClient _client = App._client;
    
    private static int _selectedAccountIndex;
    private static int _previousSelectedAccountIndex;
    private static List<long> _accountIds;

    public static Task<int> GetSelectedAccount()
    {
        var settings = SettingsService.LoadAccountSettings();
        _selectedAccountIndex = -1;
        
        if (settings.AccountIds is { Count: > 0 }) _selectedAccountIndex = settings.SelectedAccount;
        else _selectedAccountIndex = 0;

        return Task.FromResult(_selectedAccountIndex);
    }

    public static async Task AddNewAccount(MainWindow window, Frame frame)
    {
        await App.DestroyClient();
        await App.CreateClient();
        frame.Navigate(typeof(LoginView));
        if (frame.Content is LoginView login) login._window = window;
        _previousSelectedAccountIndex = _selectedAccountIndex;
        _selectedAccountIndex++;
        var settings = SettingsService.LoadAccountSettings();
        settings.SelectedAccount = _selectedAccountIndex;
        SettingsService.SaveAccountSettings(settings);
    }

    public static async Task SwitchAccount(MainWindow window, long accountId)
    {
        var settings = SettingsService.LoadAccountSettings();
        _accountIds = settings.AccountIds;
        
        await App.DestroyClient();
        
        _selectedAccountIndex = _accountIds.IndexOf(accountId);
        App._accountIndexToLogin = _selectedAccountIndex;
        settings.SelectedAccount = _selectedAccountIndex;
        
        SettingsService.SaveAccountSettings(settings);
        SettingsService.UpdateAccountInfoPath();
        
        await App.CreateClient();
        await window.UpdateWindow(App._client);
    }
}