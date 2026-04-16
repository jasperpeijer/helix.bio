using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Helix.Bio.UI.ViewModels;

public partial class SequenceProfilerViewModel : WorkspaceTabViewModel
{
    [ObservableProperty]
    private string? _dnaInput = "ATGCGTNNCGTAGCTAGCTAGCATCGATCGATCGA";
    
    [ObservableProperty]
    private string? _statusMessage = "Ready";

    [ObservableProperty]
    private string? _statusColor = "Gray"; 

    [ObservableProperty]
    private string? _stringMemoryText = "";

    [ObservableProperty]
    private string? _packedMemoryText = "";

    [ObservableProperty]
    private string? _decodedText = "";

    [ObservableProperty] private string? gcContentText = "";

    [RelayCommand]
    private void PackSequence()
    {
        if (string.IsNullOrEmpty(DnaInput)) return;

        try
        {
            int stringBytes = DnaInput.Length * 2;
            var dna = new DnaSequence(DnaInput.AsSpan());
            int packedBytes = dna.GetMemoryFootprintBytes();

            StatusMessage = "Status: Successfully Packed!";
            StatusColor = "LightGreen";
            StringMemoryText = $"Standard String Memory (UTF-16): {stringBytes} bytes";
            PackedMemoryText =
                $"4-Bit Packed Memory: {packedBytes} bytes ({(1 - (double)packedBytes / stringBytes):P0} smaller)";
            DecodedText = $"Decoded Verification: {dna}";
            GcContentText = $"GC-Content: {dna.GcContent:P2}";
        }
        catch (Exception e)
        {
            StatusMessage = $"Error: {e.Message}";
            StatusColor = "Red";
            StringMemoryText = "";
            PackedMemoryText = "";
            DecodedText = "";
        }
    }

    [RelayCommand]
    private void ReverseComplementSequence()
    {
        if (string.IsNullOrEmpty(DnaInput)) return;

        try
        {
            var dna = new DnaSequence(DnaInput.AsSpan());
            var rcDna = dna.ReverseComplement();
            DnaInput = rcDna.ToString();
            PackSequence();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            StatusColor = "Red";
        }
    }
}