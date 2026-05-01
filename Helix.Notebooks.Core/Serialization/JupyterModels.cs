using System.Text.Json;
using System.Text.Json.Serialization;

namespace Helix.Notebooks.Core.Serialization;

public class JupyterNotebookModel
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, JsonElement> Metadata { get; set; } = new();
    
    [JsonPropertyName("nbformat")]
    public int NbFormat { get; set; } = 4;
    
    [JsonPropertyName("nbformat_minor")]
    public int NbFormatMinor { get; set; } = 2;
    
    [JsonPropertyName("cells")] 
    public List<JupyterCellModel> Cells { get; set; } = [];
}

public class JupyterCellModel
{
    [JsonPropertyName("cell_type")] 
    public string CellType { get; set; } = "code";
    
    [JsonPropertyName("metadata")]
    public Dictionary<string, JsonElement> Metadata { get; set; } = new();
    
    [JsonPropertyName("source")]
    public JsonElement SourceElement { get; set; }

    [JsonIgnore]
    public List<string> Source
    {
        get
        {
            if (SourceElement.ValueKind == JsonValueKind.Array)
            {
                return SourceElement.EnumerateArray()
                    .Select(e => e.GetString() ?? "")
                    .ToList();
            }

            if (SourceElement.ValueKind == JsonValueKind.String)
            {
                return [SourceElement.GetString() ?? ""];
            }

            return [];
        }
    }
    
    [JsonPropertyName("outputs")]
    public List<object>? Outputs { get; set; }
    
}