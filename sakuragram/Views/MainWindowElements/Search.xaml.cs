using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace sakuragram.Views.MainWindowElements;

public partial class Search
{
    public Search()
    {
        InitializeComponent();
    }

    private void UIElement_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var border = new Border();
        border.Background = new SolidColorBrush(Colors.Red);
        border.CornerRadius = new CornerRadius(0, 0, 4, 4);
        border.Height = 300;
        border.Width = Width;
        Children.Add(border);
    }

    private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var border = new Border();
        border.Background = new SolidColorBrush(Colors.Red);
        border.CornerRadius = new CornerRadius(0, 0, 4, 4);
        border.Height = 300;
        border.Width = Width;
        Children.Add(border);
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var border = new Border();
        border.Background = new SolidColorBrush(Colors.Red);
        border.CornerRadius = new CornerRadius(0, 0, 4, 4);
        border.Height = 300;
        border.Width = Width;
        Children.Add(border);
    }
}