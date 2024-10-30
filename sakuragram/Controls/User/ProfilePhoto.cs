using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Controls.User;

public class ProfilePhoto : PersonPicture
{
    public static ProfilePhoto CreateInstance()
    {
        return new ProfilePhoto();
    }
}