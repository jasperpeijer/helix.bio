using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Helix.Notebooks.Core;

public partial class MarkdownCell : NotebookCell
{
    [ObservableProperty] 
    public bool _isEditing = true;
    
    public override Task ExecuteAsync()
    {
        IsEditing = false;
        State = CellState.Success;

        return Task.CompletedTask;
    }

    [RelayCommand]
    public void Edit()
    {
        IsEditing = true;
        State = CellState.Ready;
    }
}