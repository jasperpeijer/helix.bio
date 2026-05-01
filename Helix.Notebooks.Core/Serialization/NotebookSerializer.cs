using System.Text.Json;
using CommunityToolkit.Mvvm.Input;

namespace Helix.Notebooks.Core.Serialization;

public static class NotebookSerializer
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task LoadAsync(NotebookSession session, string filePath)
    {
        if (!File.Exists(filePath)) return;
        
        string json = await File.ReadAllTextAsync(filePath);
        var notebookModel = JsonSerializer.Deserialize<JupyterNotebookModel>(json, _jsonOptions);

        if (notebookModel == null) return;
        
        session.Cells.Clear();

        foreach (var jupyterCell in notebookModel.Cells)
        {
            string combinedText = string.Join("", jupyterCell.Source).TrimEnd();

            if (jupyterCell.Metadata.TryGetValue("helix_type", out JsonElement helixTypeToken))
            {
                string helixType = helixTypeToken.GetString()?.ToLower() ?? "";

                if (helixType == "latex")
                {
                    // session.Cells.Add(new LatexCell { SourceText = combinedText });
                    continue;
                }

                if (helixType == "html")
                {
                    // session.Cells.Add(new HtmlCell { SourceText = combinedText });
                    continue;
                }
            }

            if (jupyterCell.CellType == "markdown")
            {
                session.Cells.Add(new MarkdownCell() { SourceText = combinedText, IsEditing = false });
            }
            else
            {
                session.Cells.Add(new PythonCodeCell() { SourceText = combinedText });
            }
        }
    }

    public static async Task SaveAsync(NotebookSession session, string filePath)
    {
        var notebookModel = new JupyterNotebookModel();

        foreach (var cell in session.Cells)
        {
            var lines = cell.SourceText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(line => line + "\n")
                .ToList();

            var jupyterCell = new JupyterCellModel
            {
                SourceElement = JsonSerializer.SerializeToElement(lines)
            };

            if (cell is MarkdownCell)
            {
                jupyterCell.CellType = "markdown";
            }
            else if (cell is PythonCodeCell)
            {
                jupyterCell.CellType = "code";
                jupyterCell.Outputs = [];
            }
            
            notebookModel.Cells.Add(jupyterCell);
        }
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(notebookModel, options);
        await File.WriteAllTextAsync(filePath, json);
    }
    
}