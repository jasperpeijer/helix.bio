using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Helix.Bio.UI.ViewModels;

public partial class TextEditorViewModel : WorkspaceTabViewModel
{
    public string FilePath { get; }

    [ObservableProperty] private string _textContent = string.Empty;

    public TextEditorViewModel(string filePath)
    {
        FilePath = filePath;
        TabTitle = Path.GetFileName(filePath);

        LoadFile();
    }

    private void LoadFile()
    {
        if (File.Exists(FilePath))
        {
            TextContent = File.ReadAllText(FilePath);
        }
    }
}