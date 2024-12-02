using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using sakuragram.Services;
using TdLib;

namespace sakuragram.Controls.Core;

public partial class SakuraTextBlock
{
    /// <summary>
    /// A text block that displays a formatted text from the TDLib with formatting, links, and other features.
    /// </summary>
    public SakuraTextBlock() => InitializeComponent();
    
    /// <summary>
    /// Sets the formatted text of the <see cref="SakuraTextBlock"/>.
    /// </summary>
    /// <param name="formattedText">The formatted text to display.</param>
    /// <remarks>
    /// This method formats the text using <see cref="MessageService.GetTextEntities"/> and
    /// adds the result to the <see cref="TextBlock.Inlines"/> collection.
    /// </remarks>
    public Task SetFormattedText(TdApi.FormattedText formattedText)
    {
        DispatcherQueue.EnqueueAsync(() =>
        {
            var span = MessageService.GetTextEntities(formattedText);
            TextBlock.Inlines.Add(span.Result);
        });
        return Task.CompletedTask;
    }
}