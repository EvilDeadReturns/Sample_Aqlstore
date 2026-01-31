namespace Sample_Aqlstore;

public partial class AqlViewerPage : ContentPage
{
    public AqlViewerPage(string aqlContent)
    {
        InitializeComponent();
        AqlEditor.Text = aqlContent;
    }
}
