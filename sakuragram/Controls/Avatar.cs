using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Controls;

public class Avatar : PersonPicture
{
    public static Avatar CreateInstance()
    {
        return new Avatar();
    }
}