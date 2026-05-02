using System.IO;
using Helix.Notebooks.Core;

namespace Helix.Bio.UI.ViewModels;

public class JupyterWorkspaceTabViewModel : ToolWorkspaceTabViewModel
{
    public NotebookSession ActiveSession { get; }

    public JupyterWorkspaceTabViewModel(string? filePath, string? workspaceRoot, IPythonKernel? kernel)
    {
        ActiveSession = new NotebookSession(kernel);
        ActiveSession.WorkspaceRootPath = workspaceRoot;

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