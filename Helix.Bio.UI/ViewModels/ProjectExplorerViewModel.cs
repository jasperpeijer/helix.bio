using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Helix.Bio.UI.ViewModels;

public partial class ProjectExplorerViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<FileSystemNode> RootNodes { get; } = [];

    private FileSystemWatcher? _watcher;
    private string _currentDirectory = string.Empty;
    
    // [ObservableProperty] private string _currentProjectPath = "No workspace Loaded";

    public void LoadDirectory(string path)
    {
        _currentDirectory = path;
        RootNodes.Clear();
        
        if (!Directory.Exists(path)) return;

        var rootNode = new FileSystemNode(path, true);
        rootNode.IsExpanded = true;
        RootNodes.Add(rootNode);
        InitializeFileSystemWatcher(path);
        // CurrentProjectPath = Path.GetFileName(path);
        //
        // if (string.IsNullOrEmpty(CurrentProjectPath)) CurrentProjectPath = path;
        //
        // var rootNode = new FileSystemNode(path, true);
        //
        // RootNodes.Add(rootNode);
    }

    private void InitializeFileSystemWatcher(string path)
    {
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(path)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileSystemChanged;
        _watcher.Deleted += OnFileSystemChanged;
        _watcher.Renamed += OnFileSystemChanged;
    }

    private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (RootNodes.Count > 0)
            {
                _watcher!.EnableRaisingEvents = false;
                
                var expandedPaths = new HashSet<string>();
                SaveExpandedState(RootNodes[0], expandedPaths);
                
                LoadDirectory(_currentDirectory);

                if (RootNodes.Count > 0)
                {
                    RestoreExpandedState(RootNodes[0], expandedPaths);
                }
                
                _watcher.EnableRaisingEvents = true;
            }
        });
    }

    private void SaveExpandedState(FileSystemNode node, HashSet<string> expandedPaths)
    {
        if (node.IsExpanded)
        {
            expandedPaths.Add(node.FullPath);

            foreach (var child in node.Children)
            {
                SaveExpandedState(child, expandedPaths);
            }
        }
    }

    private void RestoreExpandedState(FileSystemNode node, HashSet<string> expandedPaths)
    {
        if (expandedPaths.Contains(node.FullPath))
        {
            node.IsExpanded = true;

            foreach (var child in node.Children)
            {
                RestoreExpandedState(child, expandedPaths);
            }
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}