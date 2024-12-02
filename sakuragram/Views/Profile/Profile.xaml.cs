using System.Threading.Tasks;
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
        if (user != null)
        {
            DialogProfile.Title = "User info";

            #region variables

            var userFullInfo = await _client.GetUserFullInfoAsync(user.Id);

            TdApi.Chat userPersonalChat = null;
            if (userFullInfo.PersonalChatId != 0)
            {
                userPersonalChat = await _client.GetChatAsync(userFullInfo.PersonalChatId);
            }

            if (userFullInfo.BotInfo != null)
            {
                
            }

            #endregion
            
            await ProfilePhoto.InitializeProfilePhoto(user, sizes: _photoSizes);
            TextBlockUsername.Text = string.IsNullOrEmpty(user.LastName)
                ? user.FirstName
                : $"{user.FirstName} {user.LastName}";
            TextBlockStatus.Text = UserService.GetUserStatus(user.Status);

            if (userPersonalChat != null)
            {
                await PhotoPersonalChat.InitializeProfilePhoto(chat: userPersonalChat, sizes: _photoSizes - 8);
                TextBlockPersonalChatTitle.Text = userPersonalChat.Title;
                TextBlockLastMessage.Text = await MessageService.GetTextMessageContent(userPersonalChat.LastMessage);
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
                var textBlock = new SakuraTextBlock();
                await textBlock.SetFormattedText(userFullInfo.Bio);
                CardBio.Header = textBlock;
            }
            else CardBio.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(user.Usernames.EditableUsername))
                CardUsername.Header = "@" + user.Usernames.EditableUsername;
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
        else if (chat != null)
        {
            SeparatorPersonalChat.Visibility = Visibility.Collapsed;
            GridPersonalChatInfo.Visibility = Visibility.Collapsed;
            CardMobile.Visibility = Visibility.Collapsed;
            ExpanderBusinessHours.Visibility = Visibility.Collapsed;
            CardDateOfBirth.Visibility = Visibility.Collapsed;
            CardLocation.Visibility = Visibility.Collapsed;
            
            await ProfilePhoto.InitializeProfilePhoto(chat: chat, sizes: _photoSizes);
            TextBlockUsername.Text = TextService.Truncate(chat.Title, 32);
            CardChatId.Header = chat.Id;

            switch (chat.Type)
            {
                case TdApi.ChatType.ChatTypeSupergroup typeSupergroup:
                    var fullInfo = await _client.GetSupergroupFullInfoAsync(typeSupergroup.SupergroupId);
                    
                    DialogProfile.Title = typeSupergroup.IsChannel 
                        ? "Channel info"
                        : "Group info";
                    TextBlockStatus.Text = typeSupergroup.IsChannel 
                        ? $"{fullInfo.MemberCount} subscribers"
                        : $"{fullInfo.MemberCount} members";
                    
                    if (fullInfo.InviteLink != null)
                    {
                        CardUsername.Description = "Link";
                        CardUsername.Header = fullInfo.InviteLink.IsPrimary
                            ? fullInfo.InviteLink.InviteLink
                            : string.IsNullOrEmpty(fullInfo.InviteLink.Name)
                                ? fullInfo.InviteLink.InviteLink
                                : fullInfo.InviteLink.Name;
                    }
                    else CardUsername.Visibility = Visibility.Collapsed;

                    if (!string.IsNullOrEmpty(fullInfo.Description)) 
                    {
                        var textBlock = new SakuraTextBlock();
                        var description = await _client.GetTextEntitiesAsync(fullInfo.Description);
                        await textBlock.SetFormattedText(new TdApi.FormattedText { Text = fullInfo.Description, Entities = description.Entities });
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

                    if (!string.IsNullOrEmpty(basicFullInfo.Description)) CardBio.Header = basicFullInfo.Description;
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