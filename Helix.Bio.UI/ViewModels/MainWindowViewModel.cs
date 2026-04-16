using System;
using System.Collections.ObjectModel;
using System.IO;
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
        OpenTestTab();
        string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Explorer.LoadDirectory(docsPath);
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
    private void OpenTestTab()
    {
        for (var i = 0; i < 6; i++)
        {
            Random rand = new Random();
            
            var newTool = new TestControlViewModel()
            {
                TabTitle = "Test Tab" + rand.Next(100)
            };

            OpenTabs.Add(newTool);
        }
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
    }
}