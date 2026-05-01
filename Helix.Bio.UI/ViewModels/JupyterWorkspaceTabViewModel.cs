using System.IO;
using Helix.Notebooks.Core;

namespace Helix.Bio.UI.ViewModels;

public class JupyterWorkspaceTabViewModel : ToolWorkspaceTabViewModel
{
    public NotebookSession ActiveSession { get; } = new();

    public JupyterWorkspaceTabViewModel(string? filePath = null)
    {
        ActiveSession = new NotebookSession();

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            TabTitle = Path.GetFileName(filePath);
            TabTooltip = filePath;
            _ = ActiveSession.LoadFromFileAsync(filePath);
        }
        else
        {
            TabTitle = "Untitled.ipynb";
        }
    }
}