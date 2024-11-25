using TdLib;

namespace sakuragram.Views.Messages.MessageElements;

public partial class DocumentType
{
    public DocumentType(TdApi.Document document)
    {
        InitializeComponent();
        
        TextBlockDocumentTitle.Text = document.FileName;
        TextBlockDocumentSize.Text = document.Document_.Size / 1024 / 1024 + " MB";
    }
}