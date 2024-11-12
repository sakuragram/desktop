using System;
using System.Threading.Tasks;
using Windows.System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using sakuragram.Views.Auth;
using TdLib;

namespace sakuragram.Views
{
	public sealed partial class LoginView : Page
	{
		private static TdClient _client = App._client;
		private int _loginState = 0;
		public MainWindow _window;
		private Frame _contentFrame;
		private string _passwordHint;
		private int _passwordLength;

		public LoginView()
		{
			InitializeComponent();

			TdException.Visibility = Visibility.Visible;
			_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
			
			TextBlockCurrentAuthState.Text = "Your phone";
			TextBlockCurrentAuthStateDescription.Text = "Please confirm your country code and enter your phone number.";
		}

		private async Task ProcessUpdates(TdApi.Update update)
		{
			await DispatcherQueue.EnqueueAsync(() => TdException.Text = update.ToString());
			
			switch (update)
			{
				case TdApi.Update.UpdateAuthorizationState updateAuthorizationState:
				{
					_passwordHint = updateAuthorizationState.AuthorizationState switch
					{
						TdApi.AuthorizationState.AuthorizationStateWaitPassword password => password.PasswordHint,
						_ => string.Empty
					};
					break;
				}
				case TdApi.Update.UpdateChatFolders updateChatFolders:
				{
					App._folders = updateChatFolders.ChatFolders;
					break;
				}
			}
		}
		
		private async void button_Next_Click(object sender, RoutedEventArgs e)
		{
			switch (_loginState)
			{
				case 0:
					ButtonNext.IsEnabled = false;
					TextBoxPhoneNumber.IsEnabled = false;
					LoginProgress.Visibility = Visibility.Visible;
					try
					{
						await _client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
						{
							PhoneNumber = TextBoxPhoneNumber.Text,
						});
					}
					catch (TdException exception)
					{
						TdException.Text = exception.Message;
						LoginProgress.Visibility = Visibility.Collapsed;
						ButtonNext.IsEnabled = true;
						TextBoxPhoneNumber.IsEnabled = true;
						TdException.Visibility = Visibility.Visible;
						return;
					}
					TdException.Visibility = Visibility.Collapsed;
					LoginProgress.Visibility = Visibility.Collapsed;
					_loginState++;
					TextBlockCurrentAuthState.Text = "Phone verification";
					TextBlockCurrentAuthStateDescription.Text = "We've sent the code to the Telegram app on your other device.";
					TextBoxPhoneNumber.Visibility = Visibility.Collapsed;
					TextBoxCode.Visibility = Visibility.Visible;
					ButtonNext.IsEnabled = true;
					break;
				case 1:
					ButtonNext.IsEnabled = false;
					TextBoxCode.IsEnabled = false;
					LoginProgress.Visibility = Visibility.Visible;
					try
					{
						await _client.ExecuteAsync(new TdApi.CheckAuthenticationCode
						{
							Code = TextBoxCode.Text
						});
					}
					catch (TdException exception)
					{
						TdException.Text = exception.Message;
						LoginProgress.Visibility = Visibility.Collapsed;
						ButtonNext.IsEnabled = true;
						TextBoxCode.IsEnabled = true;
						TdException.Visibility = Visibility.Visible;
						return;
					}
					TdException.Visibility = Visibility.Collapsed;
					LoginProgress.Visibility = Visibility.Collapsed;
					TextBoxCode.Visibility = Visibility.Collapsed;
					ButtonNext.IsEnabled = true;
					if (App._passwordNeeded)
					{
						_loginState++;
						TextBoxPassword.Visibility = Visibility.Visible;
						ForgotPassword.Visibility = Visibility.Visible;
						TextBlockCurrentAuthState.Text = "Password";
						TextBlockCurrentAuthStateDescription.Text = 
							"You have Two-Step Verification enabled, so your account is protected with an additional password.";
						TextBoxPassword.PlaceholderText = $"Hint: {_passwordHint}";
					}
					else
					{
						if (_window != null)
						{
							_contentFrame = _window.RootFrame;
							_window.TopBarContent.Visibility = Visibility.Visible;
						}

						if (_contentFrame != null)
						{
							_contentFrame.Navigate(typeof(ChatsView));
						}
					}
					break;
				case 2:
					ButtonNext.IsEnabled = false;
					TextBoxPassword.IsEnabled = false;
					LoginProgress.Visibility = Visibility.Visible;
					try
					{
						await _client.ExecuteAsync(new TdApi.CheckAuthenticationPassword
						{
							Password = TextBoxPassword.Password
						});
						LoginProgress.Visibility = Visibility.Collapsed;
					}
					catch (TdException exception)
					{
						TdException.Text = exception.Message;
						LoginProgress.Visibility = Visibility.Collapsed;
						ButtonNext.IsEnabled = true;
						TextBoxPassword.IsEnabled = true;
						TdException.Visibility = Visibility.Visible;
						return;
					}
					TdException.Visibility = Visibility.Collapsed;
					_loginState = 0;
					ButtonNext.IsEnabled = true;
					if (_window != null)
					{
						_contentFrame = _window.RootFrame;
						_window.TopBarContent.Visibility = Visibility.Visible;
					}

					if (_contentFrame != null)
					{
						_contentFrame.Navigate(typeof(ChatsView));
					}
					break;
			}
		}

		private void ForgotPassword_OnClick(object sender, RoutedEventArgs e)
		{
			var window = new Auth_ForgotPassword();
			window.Activate();
		}

		private void UIElement_OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			switch (e.Key)
			{
				case VirtualKey.Enter:
				{
					button_Next_Click(sender, null);
					break;
				}
			}
		}

		private void TextBoxCode_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			if (TextBoxCode.Text.Length == 5)
			{
				button_Next_Click(null, null);
			}
		}
	}
}
