using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Octokit;
using sakuragram.Services;
using sakuragram.Services.Core;
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

	private static EventHandler<TdApi.Update> _updateHandler;
	
	public MainWindow _mWindow;
	
	public static TdClient _client;
	private static List<long> _clientIDs = [];
	private static long _userToLogin = -1;
	public static GitHubClient _githubClient;

	private static TdLogLevel _logLevel = TdLogLevel.Fatal;
	public static TdApi.ChatFolderInfo[] _folders = [];
	private static readonly ManualResetEventSlim ReadyToAuthenticate = new();

	public static UpdateManager UpdateManager = new();
	public static ChatService ChatService = new();

	public static bool _authNeeded;
	public static bool _passwordNeeded;
	public static bool _hasInternetConnection = true;
	
	private static readonly int _logFileMaxSize = 4096;
	private static readonly string _logFilePath = Path.Combine(AppContext.BaseDirectory, @$"{Config.BaseLocation}\log.txt");
	private static readonly string _filesDirectory = Path.Combine(AppContext.BaseDirectory, @$"{Config.BaseLocation}\files\");

	public static int _folderId = -1;

	private static Task PrepareApplication()
	{
		#region Variables prepairing

		var settings = SettingsService.LoadSettings();
		
		_clientIDs = settings.ClientIDs;

		#endregion
		
		#region Telegram JSON Client

		using var jsonClient = new TdJsonClient();
		const string json = "";
		jsonClient.Send(json);

		#endregion

		#region Telegram Client

		
		_client = new TdClient();
		_client.Bindings.SetLogVerbosityLevel(_logLevel);
		_client.Bindings.SetLogFilePath(_logFilePath);
		_client.Bindings.SetLogFileMaxSize(_logFileMaxSize);

		#endregion

		#region GitHub Client

		_githubClient = new GitHubClient(new ProductHeaderValue(Config.AppName));
		_githubClient.Credentials = new Credentials(Config.GitHubAuthToken);

		#endregion

		#region Telegram Update Handler

		_updateHandler = UpdateHandler;
		_client.UpdateReceived += _updateHandler;

		#endregion

		ReadyToAuthenticate.Wait();
		return Task.CompletedTask;
	}

	private static async void UpdateHandler(object _, TdApi.Update update)
	{
		await ProcessUpdates(update);
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
				// int userIndex = 0;
				// if (_userToLogin > -1)
				// {
				// 	userIndex = _clientIDs.IndexOf(_userToLogin);
				// }
				// else
				// {
				// 	var waitSettings = SettingsService.LoadSettings();
				// 	if (_clientIDs.Contains(waitSettings.ClientIDs[waitSettings.ClientIndex]))
				// 		userIndex = _clientIDs.IndexOf(waitSettings.ClientIDs[waitSettings.ClientIndex]);
				// }
				
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
					DatabaseDirectory = Path.Combine(AppContext.BaseDirectory, @$"{Config.BaseLocation}\u_0"),
					FilesDirectory = _filesDirectory,
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
			case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateReady }:
				var user = await _client.ExecuteAsync(new TdApi.GetMe());
				if (_clientIDs.Contains(user.Id)) return;
				_clientIDs.Add(user.Id);
				var readySettings = SettingsService.LoadSettings();
				SettingsService.SaveSettings(new Settings
				{
					AutoUpdate = readySettings.AutoUpdate,
					InstallBeta = readySettings.InstallBeta,
					Language = readySettings.Language,
					ChatBottomFastAction = readySettings.ChatBottomFastAction,
					StartMediaPage = readySettings.StartMediaPage,
					ClientIDs = _clientIDs,
					ClientIndex = readySettings.ClientIndex
				});
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

	public static async Task AddNewAccount()
	{
		
		_client.UpdateReceived -= _updateHandler;
		_updateHandler = null;
		await _client.CloseAsync();
		
		_client = null;
		_client = new TdClient { TimeoutToClose = new TimeSpan(1, 0, 0) };
		_client.Bindings.SetLogVerbosityLevel(_logLevel);
		_client.Bindings.SetLogFilePath(_logFilePath);
		_client.Bindings.SetLogFileMaxSize(_logFileMaxSize);

		_updateHandler = UpdateHandler;
		_client.UpdateReceived += _updateHandler;
		
		var settings = SettingsService.LoadSettings();
		SettingsService.SaveSettings(new Settings
		{
			AutoUpdate = settings.AutoUpdate,
			InstallBeta = settings.InstallBeta,
			Language = settings.Language,
			ChatBottomFastAction = settings.ChatBottomFastAction,
			StartMediaPage = settings.StartMediaPage,
			ClientIDs = settings.ClientIDs,
			ClientIndex = _clientIDs.IndexOf(_client.GetMeAsync().Result.Id)
		});
	}

	public static async Task SwitchAccount(long id)
	{
		_userToLogin = id;
		
		_client.UpdateReceived -= _updateHandler;
		_updateHandler = null;
		await _client.DestroyAsync();
		
		_client = null;
		_client = new TdClient();
		_client.Bindings.SetLogVerbosityLevel(_logLevel);
		_client.Bindings.SetLogFilePath(_logFilePath);
		_client.Bindings.SetLogFileMaxSize(_logFileMaxSize);
		
		_updateHandler = UpdateHandler;
		_client.UpdateReceived += _updateHandler;
	}

	public static async Task RemoveNewAccount()
	{
		_client.UpdateReceived -= _updateHandler;
		_updateHandler = null;
		await _client.DestroyAsync();
		
		_client = null;
		_client = new TdClient();
		_client.Bindings.SetLogVerbosityLevel(_logLevel);
		_client.Bindings.SetLogFilePath(_logFilePath);
		_client.Bindings.SetLogFileMaxSize(_logFileMaxSize);
		
		_updateHandler = UpdateHandler;
		_client.UpdateReceived += _updateHandler;
		
		var settings = SettingsService.LoadSettings();
		SettingsService.SaveSettings(new Settings
		{
			AutoUpdate = settings.AutoUpdate,
			InstallBeta = settings.InstallBeta,
			Language = settings.Language,
			ChatBottomFastAction = settings.ChatBottomFastAction,
			StartMediaPage = settings.StartMediaPage,
			ClientIDs = _clientIDs,
			ClientIndex = _clientIDs.IndexOf(_client.GetMeAsync().Result.Id)
		});
	}
	
	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		await PrepareApplication();

		_mWindow = new MainWindow();
		_mWindow.Activate();
	}
}