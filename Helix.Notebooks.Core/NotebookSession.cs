using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helix.Notebooks.Core.Serialization;
using Python.Runtime;

namespace Helix.Notebooks.Core;

public partial class NotebookSession : ObservableObject, IDisposable
{
    public ObservableCollection<NotebookCell> Cells { get; } = [];
    
    private readonly IPythonKernel _kernel;
    private readonly PyModule _myScope;

    private CancellationTokenSource? _debounceCts;

    [ObservableProperty] 
    private string? _currentFilePath;

    [ObservableProperty] 
    private string _saveStatus = "Unsaved";

    public NotebookSession(IPythonKernel kernel)
    {
        _kernel = kernel;
        _myScope = kernel.CreateScope();
        Cells.CollectionChanged += Cells_CollectionChanged;
        AddPythonCell();
    }

    private void Cells_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (NotebookCell cell in e.NewItems)
            {
                cell.OnContentChanged += NotifyContentChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (NotebookCell cell in e.OldItems)
            {
                cell.OnContentChanged -= NotifyContentChanged;
            }
        }
    }

    public void NotifyContentChanged()
    {
        SaveStatus = "Unsaved";

        if (string.IsNullOrWhiteSpace(CurrentFilePath)) return;
        
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();

        _ = DebouncedSaveAsync(_debounceCts.Token);
    }

    private async Task DebouncedSaveAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(2500, token);

            if (!token.IsCancellationRequested && !string.IsNullOrWhiteSpace(CurrentFilePath))
            {
                SaveStatus = "Saving...";
                await NotebookSerializer.SaveAsync(this, CurrentFilePath);
                SaveStatus = $"Autosaved at {DateTime.Now:t}";
            }
        }
        catch (OperationCanceledException) { }
    }

    public async Task LoadFromFileAsync(string filePath)
    {
        CurrentFilePath = filePath;
        SaveStatus = "Loading...";
        await NotebookSerializer.LoadAsync(this, filePath);
        SaveStatus = "Saved";
    }

    [RelayCommand]
    public async Task ManualSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentFilePath)) return;
        
        _debounceCts?.Cancel();
        
        SaveStatus = "Saving...";
        await NotebookSerializer.SaveAsync(this, CurrentFilePath);
        SaveStatus = $"Saved at {DateTime.Now:t}";
    }

    [RelayCommand]
    public void AddPythonCell() => Cells.Add(new PythonCodeCell());

    [RelayCommand]
    public void AddMarkdownCell() => Cells.Add(new MarkdownCell());

    [RelayCommand]
    public async Task RunAllCellsAsync()
    {
        foreach (var cell in Cells)
        {
            if (cell is PythonCodeCell pythonCell)
            {
                await pythonCell.ExecuteAsync(_kernel, _myScope);
            }
            else
            {
                await cell.ExecuteAsync();
            }
        }
    }

    [RelayCommand]
    public async Task RunSingleCellAsync(NotebookCell cell)
    {
        if (cell is PythonCodeCell pythonCell)
        {
            await pythonCell.ExecuteAsync(_kernel, _myScope);
        }
        else
        {
            await cell.ExecuteAsync();
        }
    }
    
    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _kernel?.Dispose();
        Cells.CollectionChanged -= Cells_CollectionChanged;
    }

}