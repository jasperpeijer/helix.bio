using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Helix.Bio.UI.Views;

namespace Helix.Bio.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<OpenFileMessage>
{
    public ObservableCollection<WorkspaceTabViewModel> OpenTabs { get; } = [];
    public ProjectExplorerViewModel Explorer { get; } = new();
    
    [ObservableProperty] private WorkspaceTabViewModel? _activeTab;

    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
        OpenNewSequenceProfiler();
        string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Explorer.LoadDirectory(docsPath);

        var newJupyter = new JupyterWorkspaceTabViewModel()
        {
            TabTitle = "Jupyter"
        };
        OpenTabs.Add(newJupyter);
        
        var newJupyter2 = new JupyterWorkspaceTabViewModel()
        {
            TabTitle = "Jupyter"
        };
        OpenTabs.Add(newJupyter2);
    }

    [RelayCommand]
    private void OpenNewSequenceProfiler()
    {
        var newTool = new SequenceProfilerViewModel
        {
            TabTitle = "Sequence Profiler"
        };
        
        OpenTabs.Add(newTool);
        ActiveTab = newTool;
    }

    [RelayCommand]
    private void CloseTab(WorkspaceTabViewModel tabToClose)
    {
        int index = OpenTabs.IndexOf(tabToClose);

        if (index == -1) return;

        if (ActiveTab == tabToClose)
        {
            if (OpenTabs.Count > 1)
            {
                int fallbackIndex = index == 0 ? 1 : index - 1;
                ActiveTab = OpenTabs[fallbackIndex];
            }
            else
            {
                ActiveTab = null;
            }
        }
        
        tabToClose.Close();
        OpenTabs.Remove(tabToClose);
        DisambiguateTabs();
    }

    [RelayCommand]
    private void NextTab()
    {
        if (OpenTabs.Count <= 1 || ActiveTab == null) return;
        int index = OpenTabs.IndexOf(ActiveTab);
        ActiveTab = OpenTabs[(index + 1) % OpenTabs.Count];
    }

    [RelayCommand]
    private void PreviousTab()
    {
        if (OpenTabs.Count <= 1 || ActiveTab == null) return;
        int index = OpenTabs.IndexOf(ActiveTab);
        ActiveTab = index == 0 ? OpenTabs[^1] : OpenTabs[index - 1];
    }

    public void Receive(OpenFileMessage message)
    {
        if (message.FilePath.EndsWith(".ipynb", System.StringComparison.OrdinalIgnoreCase))
        {
            foreach (var tab in OpenTabs)
            {
                if (tab is JupyterWorkspaceTabViewModel jupyterTab &&
                    jupyterTab.ActiveSession.CurrentFilePath == message.FilePath)
                {
                    ActiveTab = tab;
                    return;
                }
            }

            var newNotebook = new JupyterWorkspaceTabViewModel(message.FilePath);
            OpenTabs.Add(newNotebook);
            ActiveTab = newNotebook;
            return;
        }
        
        foreach (var tab in OpenTabs)
        {
            if (tab is TextEditorViewModel editorTab && editorTab.FilePath == message.FilePath)
            {
                ActiveTab = tab;
                return;
            }
        }

        var newEditor = new TextEditorViewModel(message.FilePath);
        OpenTabs.Add(newEditor);
        ActiveTab = newEditor;
        DisambiguateTabs();
    }

    private void DisambiguateTabs()
    {
        var textTabs = OpenTabs.OfType<TextEditorViewModel>().ToList();
        var grouped = textTabs.GroupBy(t => Path.GetFileName(t.FilePath));

        foreach (var group in grouped)
        {
            var tabs = group.ToList();

            if (tabs.Count == 1)
            {
                tabs[0].UpdateBaseTitle(Path.GetFileName(tabs[0].FilePath));
            }
            else
            {
                int partsToKeep = 1;
                bool allUnique = false;

                while (!allUnique && partsToKeep < 10)
                {
                    partsToKeep++;
                    var nameCounts = tabs.GroupBy(t => GetPathSuffix(t.FilePath, partsToKeep)).ToList();

                    if (nameCounts.Count == tabs.Count)
                    {
                        foreach (var tab in tabs)
                        {
                            tab.UpdateBaseTitle(GetPathSuffix(tab.FilePath, partsToKeep));
                        }

                        allUnique = true;
                    }
                }
            }
        }
    }

    private string GetPathSuffix(string path, int parts)
    {
        var segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        int take = System.Math.Min(segments.Length, parts);

        return string.Join("/", segments.Skip(segments.Length - take));
    }
}