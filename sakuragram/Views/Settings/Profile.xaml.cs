﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
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
    
    private int _profilePhotoFileId = 0;
    private int _personalChatIdToSet = 0;
    
    public Profile()
    {
        InitializeComponent();
        UpdateCurrentUser();
    }
    
    private async Task ProcessUpdates(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateFile updateFile:
            {
                if (updateFile.File.Id == _profilePhotoFileId)
                {
                    if (updateFile.File.Local.Path != string.Empty)
                    {
                        PersonPicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                            () => PersonPicture.ProfilePicture = new BitmapImage(new Uri(updateFile.File.Local.Path)));
                    }
                    else if (_profilePhoto.Small.Local.Path != string.Empty)
                    {
                        PersonPicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                            () => PersonPicture.ProfilePicture = new BitmapImage(new Uri(_profilePhoto.Small.Local.Path)));   
                    }
                }
                break;
            }
            
        }
    }

    private async void UpdateCurrentUser()
    {
        _currentUser = await _client.GetMeAsync();
        _currentUserFullInfo = await _client.GetUserFullInfoAsync(userId: _currentUser.Id);
        if (_currentUserFullInfo.PersonalChatId != 0)
        {
            _personalChat = await _client.GetChatAsync(chatId: _currentUserFullInfo.PersonalChatId);
            CardConnectedChannel.Description = $"Connected channel: {_personalChat.Title}";
        }
        else
        {
            CardConnectedChannel.Description = "No connected channel";
        }
        _personalChats = await _client.GetSuitablePersonalChatsAsync();
        
        PersonPicture.DisplayName = _currentUser.FirstName + " " + _currentUser.LastName;
        
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
            CardDateOfBirth.Description = 
                $"Date of birth: {_currentUserFullInfo.Birthdate.Day}.{_currentUserFullInfo.Birthdate.Month}.{_currentUserFullInfo.Birthdate.Year}";
        }
        else
        {
            ButtonRemoveDateOfBirth.IsEnabled = false;
            CardDateOfBirth.Description = "";
        }
        
        UpdateProfilePhoto();
        
        _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
    }
    
    private async void UpdateProfilePhoto()
    {
        if (_currentUser.ProfilePhoto == null)
        {
            PersonPicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                () => PersonPicture.DisplayName = _currentUser.FirstName + " " + _currentUser.LastName);
            return;
        }
        
        _profilePhoto = _currentUser.ProfilePhoto;
        _profilePhotoFileId = _currentUser.ProfilePhoto.Small.Id;
        if (_currentUser.ProfilePhoto.Small.Local.Path != "")
        {
            try
            {
                PersonPicture.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                    () => PersonPicture.ProfilePicture = new BitmapImage(new Uri(_currentUser.ProfilePhoto.Small.Local.Path)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
        {
            var file = await _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = _profilePhotoFileId,
                Priority = 1
            });
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
            var newPhoto = await _client.ExecuteAsync(new TdApi.SetProfilePhoto
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
            _profilePhotoFileId = _profilePhoto.Small.Id;
            
            var file = await _client.ExecuteAsync(new TdApi.DownloadFile
            {
                FileId = _profilePhotoFileId,
                Priority = 1
            });
        }
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
        var chat = _client.GetChatAsync(chatId).Result;
        //var supergroup = _client.GetSupergroupAsync(chatId).Result;
        
        var button = new Button();
        button.Click += (sender, args) => ButtonPersonalChatEntry_OnClick(sender, args, chatId);
        button.Margin = new Thickness(0, 4, 0, 4);
        button.Width = 350;
        button.Height = 60;
            
        var stackPanel = new StackPanel();
        stackPanel.Orientation = Orientation.Horizontal;
        stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
        stackPanel.VerticalAlignment = VerticalAlignment.Center;
        
        var chatPhoto = new PersonPicture();
        chatPhoto.Width = 40;
        chatPhoto.Height = 40;

        if (chat.Photo != null)
        {
            if (chat.Photo.Small.Local.Path != string.Empty)
            {
                chatPhoto.ProfilePicture = new BitmapImage(new Uri(chat.Photo.Small.Local.Path));
            }
            else
            {
                chatPhoto.DisplayName = chat.Title;
                var file = await _client.DownloadFileAsync(fileId: chat.Photo.Small.Id, priority: 1).WaitAsync(new CancellationToken());
                chatPhoto.ProfilePicture = new BitmapImage(new Uri(file.Local.Path));
            }
        }
        else
        {
            chatPhoto.DisplayName = chat.Title;
        }
        
        stackPanel.Children.Add(chatPhoto);
        
        var verticalStackPanel = new StackPanel();
        verticalStackPanel.Orientation = Orientation.Vertical;
        verticalStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
        verticalStackPanel.VerticalAlignment = VerticalAlignment.Center;
        
        var chatTitle = new TextBlock();
        chatTitle.Text = chat.Title;
        verticalStackPanel.Children.Add(chatTitle);
        
        var chatSubscribers = new TextBlock();
        chatSubscribers.FontSize = 12;
        chatSubscribers.Text = " subscribers";
        verticalStackPanel.Children.Add(chatSubscribers);
        
        stackPanel.Children.Add(verticalStackPanel);
        button.Content = stackPanel;
        StackPanelChats.Children.Add(button);
    }

    private async void ButtonPersonalChatEntry_OnClick(object sender, RoutedEventArgs e, long chatId)
    {
        try
        {
            await _client.ExecuteAsync(new TdApi.SetPersonalChat { ChatId = chatId });
        }
        catch (TdException exception)
        {
            Debug.WriteLine(exception.Message);
            throw;
        }
        _currentUserFullInfo = _client.GetUserFullInfoAsync(_currentUser.Id).Result;
        if (_currentUserFullInfo.PersonalChatId != 0)
        {
            _personalChat = await _client.GetChatAsync(chatId: _currentUserFullInfo.PersonalChatId);
            CardConnectedChannel.Description = $"Connected channel: {_personalChat.Title}";
        }
        ContentDialogChangeConnectedChannel.Hide();
    }
}