using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using sakuragram.Controls.Core;
using sakuragram.Services;
using sakuragram.Services.Core;
using TdLib;
using TextBlock = Microsoft.UI.Xaml.Controls.TextBlock;

namespace sakuragram.Views.Profile;

public partial class Profile
{
    private TdClient _client = App._client;
    private int _photoSizes = 46;
    
    public Profile()
    {
        InitializeComponent();
    }

    public async Task PrepareProfileWindow(TdApi.User user = null, TdApi.Chat chat = null)
    {
        if (user != null) await UpdateProfileForUser(user);
        else if (chat != null)
        {
            switch (chat.Type)
            {
                case TdApi.ChatType.ChatTypeBasicGroup or TdApi.ChatType.ChatTypeSupergroup:
                    await UpdateProfileFotChat(chat);
                    break;
                case TdApi.ChatType.ChatTypePrivate or TdApi.ChatType.ChatTypeSecret:
                    var userPrivate = await _client.GetUserAsync(chat.Id);
                    await UpdateProfileForUser(userPrivate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return;

        async Task UpdateProfileForUser(TdApi.User user)
        {
            if (user == null) return;
            DialogProfile.Title = "User info";

            #region variables

            var userFullInfo = await _client.GetUserFullInfoAsync(user.Id);
            var currentUser = await _client.GetMeAsync();

            TdApi.Chat userPersonalChat = null;
            if (userFullInfo.PersonalChatId != 0)
            {
                userPersonalChat = await _client.GetChatAsync(userFullInfo.PersonalChatId);
            }

            if (userFullInfo.BotInfo != null)
            {

            }

            #endregion

            if (user.Id == currentUser.Id) CardNotification.Visibility = Visibility.Collapsed;

            await ProfilePhoto.InitializeProfilePhoto(user, sizes: _photoSizes);
            var fullname = string.IsNullOrEmpty(user.LastName)
                ? user.FirstName
                : $"{user.FirstName} {user.LastName}";
            TextBlockUsername.Text = TextService.Truncate(fullname, 25);
            TextBlockStatus.Text = UserService.GetUserStatus(user.Status);

            if (userPersonalChat != null)
            {
                await PhotoPersonalChat.InitializeProfilePhoto(chat: userPersonalChat, sizes: _photoSizes - 8);
                TextBlockPersonalChatTitle.Text = userPersonalChat.Title;
                TextBlockLastMessage.Text =
                    await MessageService.GetTextMessageContent(userPersonalChat.LastMessage);
            }
            else
            {
                GridPersonalChatInfo.Visibility = Visibility.Collapsed;
                SeparatorPersonalChat.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber)) CardMobile.Header = "+" + user.PhoneNumber;
            else CardMobile.Visibility = Visibility.Collapsed;

            if (userFullInfo.Bio != null)
            {
                var bioTextBlock = new SakuraTextBlock();
                await bioTextBlock.SetFormattedText(userFullInfo.Bio);
                CardBio.Header = bioTextBlock;
            }
            else CardBio.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(user.Usernames.EditableUsername))
            {
                var username = $"@{user.Usernames.EditableUsername}";
                var usernameTextBlock = new SakuraTextBlock();
                var usernameFormattedText = await _client.GetTextEntitiesAsync(username);
                await usernameTextBlock.SetFormattedText(new TdApi.FormattedText
                {
                    Text = username,
                    Entities = usernameFormattedText.Entities
                });
                CardUsername.Header = usernameTextBlock;
            }
            else CardUsername.Visibility = Visibility.Collapsed;

            if (userFullInfo.BusinessInfo != null)
            {
                // TODO: Opening hours

                if (userFullInfo.BusinessInfo.Location != null)
                {
                    CardLocation.Header = userFullInfo.BusinessInfo.Location.Address;

                    if (userFullInfo.BusinessInfo.Location.Location != null)
                    {
                        CardLocation.IsClickEnabled = true;
                        CardLocation.Click += (_, _) => { };
                    }
                }
            }
            else
            {
                ExpanderBusinessHours.Visibility = Visibility.Collapsed;
                CardLocation.Visibility = Visibility.Collapsed;
            }

            if (userFullInfo.Birthdate != null)
            {
                CardDateOfBirth.Header = userFullInfo.Birthdate.Year == 0
                    ? $"{userFullInfo.Birthdate.Day}.{userFullInfo.Birthdate.Month}"
                    : $"{userFullInfo.Birthdate.Day}.{userFullInfo.Birthdate.Month}.{userFullInfo.Birthdate.Year}";
            }
            else CardDateOfBirth.Visibility = Visibility.Collapsed;

            CardChatId.Header = user.Id;
        }
        
        async Task UpdateProfileFotChat(TdApi.Chat chat)
        {
            if (chat == null) return;
            SeparatorPersonalChat.Visibility = Visibility.Collapsed;
            GridPersonalChatInfo.Visibility = Visibility.Collapsed;
            CardMobile.Visibility = Visibility.Collapsed;
            ExpanderBusinessHours.Visibility = Visibility.Collapsed;
            CardDateOfBirth.Visibility = Visibility.Collapsed;
            CardLocation.Visibility = Visibility.Collapsed;

            await ProfilePhoto.InitializeProfilePhoto(chat: chat, sizes: _photoSizes);
            TextBlockUsername.Text = TextService.Truncate(chat.Title, 25);
            CardChatId.Header = chat.Id;

            switch (chat.Type)
            {
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                    var supergroup = await _client.GetSupergroupAsync(typeSupergroup.SupergroupId);
                    var fullInfo = await _client.GetSupergroupFullInfoAsync(typeSupergroup.SupergroupId);

                    DialogProfile.Title = typeSupergroup.IsChannel
                        ? "Channel info"
                        : "Group info";
                    TextBlockStatus.Text = typeSupergroup.IsChannel
                        ? $"{fullInfo.MemberCount} subscribers"
                        : $"{fullInfo.MemberCount} members";
                    
                    if (supergroup.Usernames != null && !string.IsNullOrEmpty(supergroup.Usernames.EditableUsername))
                    {
                        CardUsername.Description = "Link";
                        var link = $"https://t.me/{supergroup.Usernames.EditableUsername}";
                        var linkTextBlock = new SakuraTextBlock();
                        var linkFormattedText = await _client.GetTextEntitiesAsync(link);
                        await linkTextBlock.SetFormattedText(new TdApi.FormattedText
                        {
                            Text = link,
                            Entities = linkFormattedText.Entities
                        });
                        CardUsername.Header = linkTextBlock;
                    }
                    else if (fullInfo.InviteLink != null)
                    {
                        CardUsername.Description = "Link";
                        var link = fullInfo.InviteLink.IsPrimary
                            ? fullInfo.InviteLink.InviteLink
                            : string.IsNullOrEmpty(fullInfo.InviteLink.Name)
                                ? fullInfo.InviteLink.InviteLink
                                : fullInfo.InviteLink.Name;
                        var linkTextBlock = new SakuraTextBlock();
                        var linkFormattedText = await _client.GetTextEntitiesAsync(link);
                        await linkTextBlock.SetFormattedText(new TdApi.FormattedText
                        {
                            Text = link,
                            Entities = linkFormattedText.Entities
                        });
                        CardUsername.Header = linkTextBlock;
                    }
                    else CardUsername.Visibility = Visibility.Collapsed;

                    if (!string.IsNullOrEmpty(fullInfo.Description))
                    {
                        var textBlock = new SakuraTextBlock();
                        var description = await _client.GetTextEntitiesAsync(fullInfo.Description);
                        await textBlock.SetFormattedText(new TdApi.FormattedText
                            { Text = fullInfo.Description, Entities = description.Entities });
                        CardBio.Header = textBlock;
                    }
                    else CardBio.Visibility = Visibility.Collapsed;

                    if (!typeSupergroup.IsChannel)
                    {
                        var supergroupMembers = await _client.GetSupergroupMembersAsync(typeSupergroup.SupergroupId,
                            new TdApi.SupergroupMembersFilter.SupergroupMembersFilterRecent(), limit: 10);

                        if (supergroupMembers != null)
                        {
                            foreach (var member in supergroupMembers.Members)
                            {
                                var card = new SettingsCard();
                                var memberUsername = new TextBlock();
                                var memberSender = await UserService.GetSender(member.MemberId);
                                memberUsername.Text = memberSender.User != null
                                    ? memberSender.User.FirstName
                                    : memberSender.Chat.Title;
                                card.Content = memberUsername;
                                ExpanderMembers.Items.Add(card);
                            }
                        }
                    }

                    break;
                case TdApi.ChatType.ChatTypeBasicGroup typeBasicGroup:
                    DialogProfile.Title = "Group info";
                    var basicFullInfo = await _client.GetBasicGroupFullInfoAsync(typeBasicGroup.BasicGroupId);

                    TextBlockStatus.Text = $"{basicFullInfo.Members.Length} members";

                    if (basicFullInfo.InviteLink != null)
                    {
                        CardUsername.Description = "Link";
                        CardUsername.Header = basicFullInfo.InviteLink.IsPrimary
                            ? basicFullInfo.InviteLink.InviteLink
                            : string.IsNullOrEmpty(basicFullInfo.InviteLink.Name)
                                ? basicFullInfo.InviteLink.InviteLink
                                : basicFullInfo.InviteLink.Name;
                    }
                    else CardUsername.Visibility = Visibility.Collapsed;

                    if (!string.IsNullOrEmpty(basicFullInfo.Description))
                        CardBio.Header = basicFullInfo.Description;
                    else CardBio.Visibility = Visibility.Collapsed;

                    if (basicFullInfo.Members != null)
                    {
                        foreach (var member in basicFullInfo.Members)
                        {
                            var card = new SettingsCard();
                            var memberUsername = new TextBlock();
                            var memberSender = await UserService.GetSender(member.MemberId);
                            memberUsername.Text = memberSender.User != null
                                ? memberSender.User.FirstName
                                : memberSender.Chat.Title;
                            card.Content = memberUsername;
                            ExpanderMembers.Items.Add(card);
                        }
                    }

                    break;
            }
        }
    }
}