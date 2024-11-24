using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Views.MainWindowElements;

public partial class CreateChat
{
    private TdClient _client = App._client;
    private List<long> _userIds = [];
    private List<CheckBox> _checkBoxes = [];
    
    public CreateChat(int chatType)
    {
        InitializeComponent();

        switch (chatType)
        {
            case 0:
                PrimaryButtonClick += async (_, _) => await CreatePrivateChat(_userIds[0]);
                Title = "Create private chat";
                ExpanderAddMembers.Header = "Select user";
                GridGroupInfo.Visibility = Visibility.Collapsed;
                DispatcherQueue.EnqueueAsync(async () => await GenerateUsers(true));
                break;
            case 1:
                PrimaryButtonClick += async (_, _) => await CreateGroupChat();
                Title = "Create group";
                TextBoxChatDescription.Text = "Enter group description";
                DispatcherQueue.EnqueueAsync(async () => await GenerateUsers(false));
                break;
            case 2:
                PrimaryButtonClick += async (_, _) => await CreateChannelChat();
                Title = "Create channel";
                TextBoxChatDescription.Text = "Enter channel description";
                ExpanderAddMembers.Visibility = Visibility.Collapsed;
                break;
            case 3:
                PrimaryButtonClick += async (_, _) => await CreateSecretChat(_userIds[0]);
                Title = "Create secret chat";
                ExpanderAddMembers.Header = "Select user";
                GridGroupInfo.Visibility = Visibility.Collapsed;
                DispatcherQueue.EnqueueAsync(async () => await GenerateUsers(true));
                break;
        }
    }

    private async Task GenerateUsers(bool isPrivate)
    {
        var contacts = await _client.GetContactsAsync();

        foreach (var contact in contacts.UserIds)
        {
            var user = await _client.GetUserAsync(contact);
            
            var card = new SettingsCard();
            card.Header = string.IsNullOrEmpty(user.LastName) ? user.FirstName : $"{user.FirstName} {user.LastName}";
            card.Description = UserService.GetUserStatus(user.Status);
            
            var checkBox = new CheckBox();
            checkBox.Checked += (_, _) =>
            {
                if (!_userIds.Contains(user.Id)) _userIds.Add(user.Id);
                if (isPrivate)
                {
                    foreach (var check in _checkBoxes.Where(check => check != checkBox))
                    {
                        check.IsChecked = false;
                        check.IsEnabled = false;
                    }
                }
            };
            checkBox.Unchecked += (_, _) =>
            {
                if (_userIds.Contains(user.Id)) _userIds.Remove(user.Id);
                if (isPrivate)
                {
                    foreach (var check in _checkBoxes)
                    {
                        if (_userIds.Contains(user.Id)) _userIds.Remove(user.Id);
                        check.IsChecked = false;
                        check.IsEnabled = true;
                    }
                }
            };
            _checkBoxes.Add(checkBox);
            card.Content = checkBox;
            ExpanderAddMembers.Items.Add(card);
        }
    }
    
    private async Task CreatePrivateChat(long userId)
    {
        await _client.CreatePrivateChatAsync(userId, true);
    }

    private async Task CreateGroupChat()
    {
        await _client.CreateNewBasicGroupChatAsync(_userIds.ToArray(), TextBoxChatName.Text);
    }
    
    private async Task CreateChannelChat()
    {
        await _client.CreateNewSupergroupChatAsync(TextBoxChatName.Text, false, true, TextBoxChatDescription.Text, 
            null, 0, false);
    }
    
    private async Task CreateSecretChat(long userId)
    {
        await _client.CreateNewSecretChatAsync(userId);
    }
}