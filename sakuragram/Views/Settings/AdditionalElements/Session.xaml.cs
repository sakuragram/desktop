using System.Diagnostics;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using sakuragram.Services.Core;
using TdLib;

namespace sakuragram.Views.Settings.AdditionalElements;

public partial class Session : Page
{
    private static readonly TdClient _client = App._client;
    private TdApi.Session _session;
    
    public Session()
    {
        InitializeComponent();
    }

    public void Update(TdApi.Session session)
    {
        _session = session;
        string loginDate = MathService.CalculateDateTime(session.LogInDate).ToShortTimeString();
        
        TextBlockApplicationName.Text = session.ApplicationName + " " + session.ApplicationVersion;
        TextBlockPlatformAndVersion.Text = session.DeviceModel;
        TextBlockLocationAndDate.Text = session.Location + ", " + loginDate;

        BorderAppColor.Background = session.ApplicationName switch
        {
            "Telegram Desktop" => new SolidColorBrush(Colors.Aqua),
            "Telegram Web" => new SolidColorBrush(Colors.Blue),
            "Telegram Android" => new SolidColorBrush(Colors.Green),
            "Telegram iOS" => new SolidColorBrush(Colors.Orange),
            "Swiftgram" => new SolidColorBrush(Colors.Red),
            "Unigram" => new SolidColorBrush(Colors.Coral),
            "sakuragram" => new SolidColorBrush(Colors.HotPink),
            _ => BorderAppColor.Background
        };
    }

    private void ButtonTerminateSession_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _client.TerminateSessionAsync(_session.Id);
        }
        catch (TdException exception)
        {
            Debug.WriteLine(exception.Message);
            throw;
        }
    }
}