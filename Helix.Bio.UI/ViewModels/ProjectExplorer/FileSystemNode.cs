using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Helix.Notebooks.Core;

namespace Helix.Bio.UI.ViewModels;

public partial class FileSystemNode : ViewModelBase
{
    public string Name { get; }
    public string FullPath { get; }
    public bool IsDirectory { get; }
    public string FileExtension { get; }

    public bool IsFasta => string.Equals(FileExtension, ".fasta", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(FileExtension, ".fa", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(FileExtension, ".fna", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(FileExtension, ".faa", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(FileExtension, ".fas", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(FileExtension, ".fsa", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(FileExtension, ".frn", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(FileExtension, ".mpfa", StringComparison.OrdinalIgnoreCase);
    
    public bool IsDefaultFile => !IsDirectory && !IsFasta && !IsNotebook;

    public bool IsNotebook => !IsDirectory && Name.EndsWith(".ipynb", StringComparison.OrdinalIgnoreCase);

    [ObservableProperty] private bool _isExpanded;

    public ObservableCollection<FileSystemNode> Children { get; } = new();
    
    private bool _hasLoadedChildren;
    
    public FileSystemNode(string fullPath, bool isDirectory)
    {
        FullPath = fullPath;
        Name = Path.GetFileName(fullPath);
        FileExtension = Path.GetExtension(fullPath);

        if (string.IsNullOrEmpty(Name)) Name = FullPath;
        
        IsDirectory = isDirectory;

        if (IsDirectory)
        {
            Children.Add(new FileSystemNode("Loading...", false));
        }
    }

    partial void OnIsExpandedChanged(bool value)
    {
        if (value && IsDirectory && !_hasLoadedChildren)
        {
            LoadChildren();
        }
    }

    private void LoadChildren()
    {
        Children.Clear();

        try
        {
            foreach (var dir in Directory.GetDirectories(FullPath))
            {
                Children.Add(new FileSystemNode(dir, true));
            }

            foreach (var file in Directory.GetFiles(FullPath))
            {
                Children.Add(new FileSystemNode(file, false));
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Silently ignore system folders we don't have permission to read
        }
        
        _hasLoadedChildren = true;
    }
}