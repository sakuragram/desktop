using System;
using System.Collections.Generic;
using System.Drawing;
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
	
	public MainWindow _mWindow;

	public static TdClient _client;
	public static GitHubClient _githubClient;
	
	public static int _accountIndexToLogin = -1;
	private bool _isClientAlreadyInitialized = false;
	
	private static readonly ManualResetEventSlim ReadyToAuthenticate = new();

	public static UpdateManager UpdateManager = new();
	public static ChatService ChatService = new();

	public static TdApi.ChatFolderInfo[] _folders = [];
	public static List<TdApi.ChatActiveStories> _stories = [];
	
	public static bool _authNeeded;
	public static bool _passwordNeeded;
	public static bool _hasInternetConnection = true;
	
	private static TdLogLevel _logLevel = TdLogLevel.Fatal;
	private static readonly int _logFileMaxSize = 4096;
	private static readonly string _logFilePath = Path.Combine(AppContext.BaseDirectory, @$"{Config.BaseLocation}\log.txt");
	private static readonly string _filesDirectory = Path.Combine(AppContext.BaseDirectory, @$"{Config.BaseLocation}\files\");

	public static int _folderId = -1;

	public static async Task PrepareApplication()
	{
		#region Variables prepairing

		_accountIndexToLogin = await AccountManager.GetSelectedAccount();

		#endregion
		
		#region Telegram JSON Client

		using var jsonClient = new TdJsonClient();
		const string json = "";
		jsonClient.Send(json);

		#endregion

		#region Telegram Client
		
		CreateClient();

		#endregion

		#region GitHub Client

		_githubClient = new GitHubClient(new ProductHeaderValue(Config.AppName));
		_githubClient.Credentials = new Credentials(Config.GitHubAuthToken);

		#endregion

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
				var dbPath = Path.Combine(AppContext.BaseDirectory, @$"{Config.BaseLocation}\u_{_accountIndexToLogin}");
				await _client.ExecuteAsync(new TdApi.SetTdlibParameters
				{
					ApiId = Config.ApiId,
					ApiHash = Config.ApiHash,
					UseTestDc = Config.IsTestDc,
					UseFileDatabase = true,
					UseChatInfoDatabase = true,
					UseMessageDatabase = true,
					UseSecretChats = true,
					DeviceModel = "Desktop",
					SystemLanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
					ApplicationVersion = Config.AppVersion,
					DatabaseDirectory = dbPath,
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
				var readyAccountSettings = SettingsService.LoadAccountSettings();
				var accountIds = readyAccountSettings.AccountIds;
				
				if (!accountIds.Contains(user.Id)) accountIds.Add(user.Id);
				SettingsService.SaveAccountSettings(new AccountSettings
				{
					SelectedAccount = _accountIndexToLogin,
					AccountIds = accountIds
				});
				
				await _client.LoadActiveStoriesAsync(new TdApi.StoryList.StoryListMain());
				break;
			case TdApi.Update.UpdateUser:
				ReadyToAuthenticate.Set();
				break;
			case TdApi.Update.UpdateChatActiveStories updateChatActiveStories:
				_stories.Add(updateChatActiveStories.ActiveStories);
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
				
				var accountInfo = SettingsService.LoadAccountInfoSettings();
				accountInfo.ChatFolders = _folders;
				SettingsService.SaveAccountInfoSettings(accountInfo);
				break;
			}
		}
	}
	public static Task CreateClient()
	{
		_client = new TdClient();
		_client.Bindings.SetLogVerbosityLevel(_logLevel);
		_client.Bindings.SetLogFilePath(_logFilePath);
		_client.Bindings.SetLogFileMaxSize(_logFileMaxSize);
		_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
		return Task.CompletedTask;
	}
	
	public static Task DestroyClient()
	{
		_client.UpdateReceived -= async (_, update) => { await ProcessUpdates(update); };
		_client = null;
		return Task.CompletedTask;
	}
	
	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		await PrepareApplication();

		_mWindow = new MainWindow();
		// TODO: Load all chat folders before starting the app and save it to user database
		if (DevelopmentConfig.ChatListVersion == 2)
		{
			if (_mWindow.ChatsView != null) await _mWindow.ChatsView.PrepareChatFolders();
		}
		_mWindow.Activate();
	}
}