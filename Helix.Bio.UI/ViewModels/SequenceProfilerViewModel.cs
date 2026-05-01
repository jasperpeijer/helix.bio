using System;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Helix.Bio.UI.ViewModels;

public partial class SequenceProfilerViewModel : ToolWorkspaceTabViewModel
{
    [ObservableProperty] private string _inputSequence = string.Empty;
    [ObservableProperty] private string _sequenceType = "Pending...";
    [ObservableProperty] private int _sequenceLength = 0;
    [ObservableProperty] private double _gcContent = 0.0;
    [ObservableProperty] private double _meltingTemperature = 0.0;
    [ObservableProperty] private double _molecularWeight = 0.0;
    [ObservableProperty] private string _warnings = string.Empty;

    public SequenceProfilerViewModel()
    {
        TabTitle = "Sequence Profiler";
        TabTooltip = "Biochemical Sequence Analysis";
    }

    [RelayCommand]
    private void AnalyzeSequence()
    {
        if (string.IsNullOrWhiteSpace(InputSequence))
        {
            Warnings = "Please provide a valid sequence";
            return;
        }

        string cleanString = Regex.Replace(InputSequence, @"\s+", "");

        try
        {
            var dna = new DnaSequence(cleanString.AsSpan());
            SequenceLength = dna.Length;
            GcContent = Math.Round(dna.GcContent * 100, 2);
            SequenceType = "DNA";
            MeltingTemperature = SequenceAnalyzer.CalculateMeltingTemperature(dna);
            MolecularWeight = SequenceAnalyzer.CalculateMolecularWeight(dna);
            Warnings = string.Empty;
        }
        catch (ArgumentException e)
        {
            Warnings = $"Validation failed: {e.Message}";
            SequenceLength = 0;
            GcContent = 0;
            MeltingTemperature = 0;
            MolecularWeight = 0;
            SequenceType = "Invalid";
        }
    }
}