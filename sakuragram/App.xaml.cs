using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Octokit;
using sakuragram.Services;
using sakuragram.Services.Core;
using sakuragram.Views;
using TdLib;
using TdLib.Bindings;
using Application = Microsoft.UI.Xaml.Application;
using TdLogLevel = TdLib.Bindings.TdLogLevel;

namespace sakuragram;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}
        
	public static TdClient _client;
	public static GitHubClient _githubClient;
	
	public static TdApi.ChatFolderInfo[] _folders = [];
	private static readonly ManualResetEventSlim ReadyToAuthenticate = new();

	public static UpdateManager UpdateManager = new();
	public static ChatService ChatService = new();
	
	public static bool _authNeeded;
	public static bool _passwordNeeded;
	public static bool _hasInternetConnection = true;

	public static int _folderId = -1;
        
	private void PrepareApplication()
	{
		using var jsonClient = new TdJsonClient();

		const string json = "";

		jsonClient.Send(json);
			
		_client = new TdClient();
		_client.Bindings.SetLogVerbosityLevel(TdLogLevel.Fatal);
		_client.Bindings.SetLogFilePath(Path.Combine(AppContext.BaseDirectory, @$"{Config.BaseLocation}\log.txt"));
		_client.Bindings.SetLogFileMaxSize(4096);
		
		_githubClient = new GitHubClient(new ProductHeaderValue(Config.AppName));
		_githubClient.Credentials = new Credentials(Config.GitHubAuthToken);
			
		_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };

		ReadyToAuthenticate.Wait();
	}

	private static async Task ProcessUpdates(TdApi.Update update)
	{
		switch (update)
		{
			case TdApi.Update.UpdateOption updateOption:
				if (updateOption.Name == "version")
					Config.TdLibVersion = updateOption.Value switch
					{
						TdApi.OptionValue.OptionValueString optionValueString => optionValueString.Value,
						_ => string.Empty
					};
				break;
			
			case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters }:
				var filesLocation = Path.Combine(AppContext.BaseDirectory, Config.BaseLocation);
				await _client.ExecuteAsync(new TdApi.SetTdlibParameters
				{
					ApiId = Config.ApiId,
					ApiHash = Config.ApiHash,
					UseFileDatabase = true,
					UseChatInfoDatabase = true,
					UseMessageDatabase = true,
					UseSecretChats = true,
					DeviceModel = "Desktop",
					SystemLanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
					ApplicationVersion = Config.AppVersion,
					DatabaseDirectory = filesLocation,
					FilesDirectory = filesLocation,
				});
				break;

			case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber }:
				_authNeeded = true;
				ReadyToAuthenticate.Set();
				break;
			case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitCode }:
				_authNeeded = true;
				ReadyToAuthenticate.Set();
				break;

			case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitPassword }:
				_authNeeded = true;
				_passwordNeeded = true;
				ReadyToAuthenticate.Set();
				break;

			case TdApi.Update.UpdateUser:
				ReadyToAuthenticate.Set();
				break;

			case TdApi.Update.UpdateConnectionState { State: TdApi.ConnectionState.ConnectionStateReady }:
				_authNeeded = false;
				_passwordNeeded = false;
				_hasInternetConnection = true;
				ReadyToAuthenticate.Set();
				break;
			case TdApi.Update.UpdateConnectionState updateConnectionState:
			{
				_hasInternetConnection = updateConnectionState.State switch
				{
					TdApi.ConnectionState.ConnectionStateConnecting => false,
					TdApi.ConnectionState.ConnectionStateUpdating => false,
					TdApi.ConnectionState.ConnectionStateConnectingToProxy => false,
					TdApi.ConnectionState.ConnectionStateWaitingForNetwork => false,
					_ => false
				};
				break;
			}
			
			case TdApi.Update.UpdateChatFolders updateChatFolders:
			{
				_folders = updateChatFolders.ChatFolders;
				break;
			}
		}
	}
        
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		PrepareApplication();
		
		if (_authNeeded)
		{
			_loginWindow = new LoginView();
			_loginWindow.Activate();
		}
		else
		{
			_mWindow = new MainWindow();
			_mWindow.Activate();
		}
	}

	public Window _mWindow;
	private Window _loginWindow;
}