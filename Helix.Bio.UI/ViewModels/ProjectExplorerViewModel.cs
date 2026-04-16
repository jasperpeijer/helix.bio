using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Helix.Bio.UI.ViewModels;

public partial class ProjectExplorerViewModel : ViewModelBase
{
    public ObservableCollection<FileSystemNode> RootNodes { get; } = [];
    
    [ObservableProperty] private string _currentProjectPath = "No workspace Loaded";

    public void LoadDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        
        RootNodes.Clear();
        CurrentProjectPath = Path.GetFileName(path);

        if (string.IsNullOrEmpty(CurrentProjectPath)) CurrentProjectPath = path;

        var rootNode = new FileSystemNode(path, true);
        
        RootNodes.Add(rootNode);
    }
}