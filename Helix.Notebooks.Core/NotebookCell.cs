using CommunityToolkit.Mvvm.ComponentModel;

namespace Helix.Notebooks.Core;

public abstract partial class NotebookCell : ObservableObject
{
    public event Action? OnContentChanged;
    
    public Guid Id { get; } = Guid.NewGuid();
    
    [ObservableProperty] private CellState _state = CellState.Ready;
    
    private string _sourceText = string.Empty;

    public string SourceText
    {
        get => _sourceText;
        set
        {
            if (SetProperty(ref _sourceText, value))
            {
                OnContentChanged?.Invoke();
            }
        }
    }
    
    [ObservableProperty] private string _outputText = string.Empty;

    public abstract Task ExecuteAsync();
}