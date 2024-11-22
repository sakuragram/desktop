using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using sakuragram.Controls.User;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Views.Settings;

public partial class Profile : Page
{
    private static TdClient _client = App._client;
    private static TdApi.User _currentUser;
    private static TdApi.UserFullInfo _currentUserFullInfo;
    private static TdApi.ProfilePhoto _profilePhoto;
    private static TdApi.Chat _personalChat;
    private static TdApi.Chats _personalChats;
    
    private int _personalChatIdToSet = 0;
    private readonly MediaService _mediaService = new();

    public Profile()
    {
        InitializeComponent();
        UpdateCurrentUser();
    }

    private async void UpdateCurrentUser()
    {
        _currentUser = await _client.GetMeAsync();
        _currentUserFullInfo = await _client.GetUserFullInfoAsync(userId: _currentUser.Id);
        if (_currentUserFullInfo.PersonalChatId != 0)
        {
            _personalChat = await _client.GetChatAsync(chatId: _currentUserFullInfo.PersonalChatId);
            ButtonRemoveConnectedChannel.IsEnabled = true;
            TextBlockConnectedChannel.Text = $"Connected channel: {_personalChat.Title}";
            TextBlockConnectedChannel.Visibility = Visibility.Visible;
        }
        else
        {
            CardConnectedChannel.Description = "No connected channel";
            TextBlockConnectedChannel.Visibility = Visibility.Visible;
        }
        _personalChats = await _client.GetSuitablePersonalChatsAsync();
        
        await PersonPicture.InitializeProfilePhoto(_currentUser, sizes: 100);
        
        TextBlockId.Text = $"ID: {_currentUser.Id}";
        TextBoxFirstName.Text = _currentUser.FirstName;
        TextBoxLastName.Text = _currentUser.LastName;
        TextBoxUsername.Text = _currentUser.Usernames.EditableUsername;
        TextBoxBio.Text = _currentUserFullInfo.Bio.Text;
        
        CardPhoneNumber.Description = $"Phone number: +{_currentUser.PhoneNumber}";
        
        if (_currentUserFullInfo.Birthdate != null)
        {
            ButtonRemoveDateOfBirth.IsEnabled = true;
            DatePicker.MaxYear = new DateTimeOffset(new DateTime(year: 2024, month: 12, day: 31));
            DatePicker.MinYear = new DateTimeOffset(new DateTime(year: 1900, month: 1, day: 1));
            DatePicker.SelectedDate = new DateTimeOffset(
                new DateTime(
                    _currentUserFullInfo.Birthdate.Year, 
                    _currentUserFullInfo.Birthdate.Month, 
                    _currentUserFullInfo.Birthdate.Day
                ));
            TextBlockBirthdate.Text = 
                $"Date of birth: {_currentUserFullInfo.Birthdate.Day}.{_currentUserFullInfo.Birthdate.Month}.{_currentUserFullInfo.Birthdate.Year}";
            TextBlockBirthdate.Visibility = Visibility.Visible;
        }
        else
        {
            ButtonRemoveDateOfBirth.IsEnabled = false;
            CardDateOfBirth.Description = "";
            TextBlockBirthdate.Visibility = Visibility.Collapsed;
        }
        
        PersonPicture.Visibility = Visibility.Visible;
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
    }

    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateUser { User: TdApi.User updateUser }:
                if (updateUser.Id == _currentUser.Id)
                {
                    _currentUser = updateUser;
                    await PersonPicture.InitializeProfilePhoto(_currentUser, sizes: 100);
                }
                break;
        }
    }

    private async void UpdateBirthDate()
    {
        await _client.ExecuteAsync(new TdApi.SetBirthdate
        {
            Birthdate = new TdApi.Birthdate
            {
                Day = DatePicker.Date.Day,
                Month = DatePicker.Date.Month,
                Year = DatePicker.Date.Year
            }
        });
        ButtonRemoveDateOfBirth.IsEnabled = true;
        CardDateOfBirth.Description = 
            $"Date of birth: {DatePicker.Date.Day}.{DatePicker.Date.Month}.{DatePicker.Date.Year}";
    }
    
    private async void ButtonLogOut_OnClick(object sender, RoutedEventArgs e)
    {
        await _client.ExecuteAsync(new TdApi.LogOut());
        await _client.CloseAsync();
        Application.Current.Exit();
    }

    private async void ButtonSave_OnClick(object sender, RoutedEventArgs e)
    {
        if (TextBoxFirstName.Text != _currentUser.FirstName || TextBoxLastName.Text != _currentUser.LastName)
        {
            await _client.ExecuteAsync(new TdApi.SetName
            {
                FirstName = TextBoxFirstName.Text,
                LastName = TextBoxLastName.Text
            });
        }
        if (TextBoxUsername.Text != _currentUser.Usernames.EditableUsername)
        {
            await _client.ExecuteAsync(new TdApi.SetUsername
            {
                Username = TextBoxUsername.Text
            });
        }
        if (TextBoxBio.Text != _currentUserFullInfo.Bio.Text)
        {
            await _client.ExecuteAsync(new TdApi.SetBio
            {
                Bio = TextBoxBio.Text
            });
        }

        if (_currentUserFullInfo.Birthdate != null)
        {
            if (DatePicker.Date.Day != _currentUserFullInfo.Birthdate.Day ||
                DatePicker.Date.Month != _currentUserFullInfo.Birthdate.Month ||
                DatePicker.Date.Year != _currentUserFullInfo.Birthdate.Year)
            {
                UpdateBirthDate();
            }
        }
        else
        {
            UpdateBirthDate();
        }
    }

    private async void ButtonRemoveDateOfBirth_OnClick(object sender, RoutedEventArgs e)
    {
        if (_currentUserFullInfo.Birthdate == null) return;
        await _client.ExecuteAsync(new TdApi.SetBirthdate
        {
            Birthdate = null
        });
        ButtonRemoveDateOfBirth.IsEnabled = false;
        DatePicker.SelectedDate = null;
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var task = SelectFile();
        await task;
    }

    private async Task SelectFile()
    {
        PersonPicture.Visibility = Visibility.Collapsed;
        var folderPicker = new FileOpenPicker();

        var mainWindow = (Application.Current as App)?._mWindow;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        folderPicker.FileTypeFilter.Add(".png");
        folderPicker.FileTypeFilter.Add(".jpg");
        folderPicker.FileTypeFilter.Add(".jpeg");
        folderPicker.FileTypeFilter.Add(".webm");
        folderPicker.FileTypeFilter.Add(".webp");
        var newPhotoFile = await folderPicker.PickSingleFileAsync();
        
        if (newPhotoFile != null)
        {
            await _client.ExecuteAsync(new TdApi.SetProfilePhoto
            {
                Photo = new TdApi.InputChatPhoto.InputChatPhotoStatic
                {
                    Photo = new TdApi.InputFile.InputFileLocal
                    {
                        Path = newPhotoFile.Path
                    }
                }, 
                IsPublic = false
            });

            _currentUser = _client.GetMeAsync().Result;
            _profilePhoto = _currentUser.ProfilePhoto;
            
            MediaService.GetUserPhoto(_currentUser, PersonPicture);
        }
        PersonPicture.Visibility = Visibility.Visible;
    }

    private void ButtonDeleteAccount_OnClick(object sender, RoutedEventArgs e)
    {
        ContentDialogAccountDeletion.ShowAsync();
    }

    private void ContentDialogAccountDeletion_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _client.DeleteAccountAsync(reason: "The user wanted to delete the account");
    }

    private void ContentDialogAccountDeletion_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        TextBlockAccountDeletionMessage.Text = "Attention: in this case, your Telegram account will be permanently deleted along with all the information that you store on Telegram servers.\n\nImportant: You can cancel the deletion and export the account data first so as not to lose it. (To do this, log in to your account using the latest version of Telegram for PC and select Settings > Export data from Telegram.)";
    }

    private void ButtonChangeConnectedChannel_OnClick(object sender, RoutedEventArgs e)
    {
        ContentDialogChangeConnectedChannel.ShowAsync();
    }

    private void ContentDialogChangeConnectedChannel_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        foreach (var chatId in _personalChats.ChatIds)
        {
            if (_currentUserFullInfo.PersonalChatId == chatId) continue;
            CreatePersonalChatEntry(chatId);
        }
    }

    private void ContentDialogChangeConnectedChannel_OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        StackPanelChats.Children.Clear();
    }
    
    private async void CreatePersonalChatEntry(long chatId)
    {
        var chat = await _client.GetChatAsync(chatId);
        
        var button = new Button();
        button.Click += async (_, _) =>
        {
            await _client.SetPersonalChatAsync(chat.Id);
            ContentDialogChangeConnectedChannel.Hide();
        };
        button.Margin = new Thickness(0, 4, 0, 4);
        button.Width = 350;
        button.Height = 60;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
        button.VerticalContentAlignment = VerticalAlignment.Center;
            
        var stackPanel = new StackPanel();
        stackPanel.Orientation = Orientation.Horizontal;
        stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
        stackPanel.VerticalAlignment = VerticalAlignment.Center;
        
        var chatPhoto = new ProfilePhoto();
        await chatPhoto.InitializeProfilePhoto(chat: chat, sizes: 40);
        chatPhoto.Margin = new Thickness(0, 0, 4, 0);
        stackPanel.Children.Add(chatPhoto);
        
        var verticalStackPanel = new StackPanel();
        verticalStackPanel.Orientation = Orientation.Vertical;
        verticalStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
        verticalStackPanel.VerticalAlignment = VerticalAlignment.Center;
        
        var chatTitle = new TextBlock();
        chatTitle.Text = chat.Title;
        chatTitle.FontSize = 12;
        chatTitle.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        verticalStackPanel.Children.Add(chatTitle);
        
        var chatSubscribers = new TextBlock();
        chatSubscribers.FontSize = 10;
        chatSubscribers.Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"];
        switch (chat.Type)
        {
            case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                if (typeSupergroup.IsChannel)
                {
                    var supergroup = await _client.GetSupergroupAsync(typeSupergroup.SupergroupId);
                    chatSubscribers.Text = $"{supergroup.MemberCount} members";
                }
                break;
            default:
                chatSubscribers.Text = "0 members";
                break;
        }
        verticalStackPanel.Children.Add(chatSubscribers);
        
        stackPanel.Children.Add(verticalStackPanel);
        button.Content = stackPanel;
        StackPanelChats.Children.Add(button);
    }
    
    private async void ButtonRemoveConnectedChannel_OnClick(object sender, RoutedEventArgs e)
    {
        if (_currentUserFullInfo.PersonalChatId != 0)
        {
            await _client.SetPersonalChatAsync();
        }
    }

    private static Visibility ChangeShimmersVisibility(Visibility vis) => vis switch
    {
        Visibility.Collapsed => Visibility.Visible,
        Visibility.Visible => Visibility.Collapsed,
        _ => throw new Exception()
    };
}