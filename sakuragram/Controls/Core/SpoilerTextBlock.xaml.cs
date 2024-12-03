using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using TdLib;

namespace sakuragram.Controls.Core;

public partial class SpoilerTextBlock
{
    private bool isSpoilerOpen = false;
    
/// <summary>
/// Updates the text block with the given formatted text.
/// </summary>
/// <param name="formattedText">The formatted text to display in the text block.</param>
/// <returns>A completed task representing the asynchronous operation.</returns>
/// <remarks>
/// This method utilizes <see cref="SakuraTextBlock.SetFormattedText"/> to set and display the formatted text within the text block.
/// </remarks>
    public Task UpdateTextBlock(TdApi.FormattedText formattedText)
    {
        TextBlock.SetFormattedText(formattedText);
        return Task.CompletedTask;
    }

    private void BorderSpoiler_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && !isSpoilerOpen)
        {
            BorderSpoiler.Visibility = Visibility.Collapsed;
        }
    }

    private void TextBlock_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && isSpoilerOpen)
        {
            BorderSpoiler.Visibility = Visibility.Visible;
        }
    }
}