using CommunityToolkit.Mvvm.ComponentModel;

namespace Helix.Bio.UI.ViewModels;

public abstract partial class WorkspaceTabViewModel : ViewModelBase
{
    [ObservableProperty] private string _tabTitle = "New Tab";
    [ObservableProperty] private bool _hasUnsavedChanges = false;
    [ObservableProperty] private string _tabTooltip = string.Empty;
    
    public virtual void Close() {}
}