using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Helix.Bio.UI.ViewModels;

public partial class TextEditorViewModel : WorkspaceTabViewModel
{
    public string FilePath { get; }

    [ObservableProperty] private string _textContent = string.Empty;
    
    [ObservableProperty] private bool _hasUnsavedChanges = false;

    public bool IsFasta => FilePath.EndsWith(".fasta", System.StringComparison.OrdinalIgnoreCase) ||
                           FilePath.EndsWith(".fa", System.StringComparison.OrdinalIgnoreCase) ||
                           FilePath.EndsWith(".fna", System.StringComparison.OrdinalIgnoreCase) ||
                           FilePath.EndsWith(".faa", System.StringComparison.OrdinalIgnoreCase) ||
                           FilePath.EndsWith(".fas", System.StringComparison.OrdinalIgnoreCase) ||
                           FilePath.EndsWith(".fsa", System.StringComparison.OrdinalIgnoreCase) ||
                           FilePath.EndsWith(".frn", System.StringComparison.OrdinalIgnoreCase) ||
                           FilePath.EndsWith(".mpfa", System.StringComparison.OrdinalIgnoreCase);
    
    public bool IsDefaultFile => !IsFasta;
    
    private bool _isLoading = true;
    private string _baseTitle = string.Empty;

    public TextEditorViewModel(string filePath)
    {
        FilePath = filePath;
        TabTooltip = filePath;
        TabTitle = Path.GetFileName(filePath);

        LoadFile();
    }

    public void UpdateBaseTitle(string newTitle)
    {
        _baseTitle = newTitle;
        TabTitle = HasUnsavedChanges ? _baseTitle + "*" : _baseTitle;
    }

    partial void OnHasUnsavedChangesChanged(bool value)
    {
        TabTitle = value ? _baseTitle + "*" : _baseTitle;
    }

    private void LoadFile()
    {
        if (File.Exists(FilePath))
        {
            _isLoading = true;
            TextContent = File.ReadAllText(FilePath);
            HasUnsavedChanges = false;
            _isLoading = false;
        }
    }

    partial void OnTextContentChanged(string value)
    {
        if (_isLoading) return;
        
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!HasUnsavedChanges) return;

        await File.WriteAllTextAsync(FilePath, TextContent);
        HasUnsavedChanges = false;

        if (TabTitle.EndsWith("*"))
        {
            TabTitle = TabTitle.TrimEnd('*');
        }
    }
}