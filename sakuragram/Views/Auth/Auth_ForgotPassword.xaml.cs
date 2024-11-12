using Microsoft.UI.Xaml;
using TdLib;

namespace sakuragram.Views.Auth;

public partial class Auth_ForgotPassword : Window
{
    private static TdClient _client = App._client;
    
    public Auth_ForgotPassword()
    {
        InitializeComponent();
        
        Title = "CherryMerryGram : Password recovery";
        ExtendsContentIntoTitleBar = true;
        TrySetDesktopAcrylicBackdrop();
        
        _client.ExecuteAsync(new TdApi.RequestAuthenticationPasswordRecovery());
    }

    private bool TrySetDesktopAcrylicBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
            this.SystemBackdrop = DesktopAcrylicBackdrop;

            return true;
        }

        return false;
    }

    private async void ButtonNext_OnClick(object sender, RoutedEventArgs e)
    {
        RequestedCode.IsEnabled = false;
        TextBoxPassword.IsEnabled = false;
        TextBoxRepeatPassword.IsEnabled = false;
        NewHint.IsEnabled = false;
        ButtonNext.IsEnabled = false;
        LoginProgress.Visibility = Visibility.Visible;
        
        await _client.ExecuteAsync(new TdApi.RecoverPassword {
            RecoveryCode = RequestedCode.Text,
            NewPassword = TextBoxPassword.Password,
            NewHint = NewHint.Text
        });
        Close();
    }

    private void RequestNewCode_OnClick(object sender, RoutedEventArgs e)
    {
        _client.ExecuteAsync(new TdApi.RequestAuthenticationPasswordRecovery());
        RequestedCode.IsEnabled = false;
    }
}