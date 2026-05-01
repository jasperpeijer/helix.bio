namespace Helix.Notebooks.Core;

public partial class PythonCodeCell : NotebookCell
{
    public async Task ExecuteAsync(PythonKernel kernel)
    {
        if (string.IsNullOrWhiteSpace(SourceText)) return;

        State = CellState.Running;
        OutputText = "Executing...";
        string result = await Task.Run(() => kernel.Execute((SourceText)));
        OutputText = result;
        State = result.StartsWith("Python Error:") ? CellState.Error : CellState.Success;
    }

    public override Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }
}