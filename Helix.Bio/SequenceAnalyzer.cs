namespace Helix.Bio;

public class SequenceAnalyzer
{
    public static double CalculateMeltingTemperature(DnaSequence sequence)
    {
        if (sequence.Length == 0) return 0;

        var a = sequence.GetCount('A');
        var c = sequence.GetCount('C');
        var g = sequence.GetCount('G');
        var t = sequence.GetCount('T');

        if (sequence.Length < 14)
        {
            // Wallace rule for short primers
            return (a + t) * 2 + (g + c) * 4;
        }
        
        // Standard equation for sequences > 13 bp
        return 64.9 + 41.0 * (g + c - 16.4) / sequence.Length;
    }

    public static double CalculateMolecularWeight(DnaSequence sequence)
    {
        if (sequence.Length == 0) return 0;

        var a = sequence.GetCount('A');
        var c = sequence.GetCount('C');
        var g = sequence.GetCount('G');
        var t = sequence.GetCount('T');

        return (a * 313.2) + (t * 304.2) + (c * 289.2) + (g * 329.2) + 79.0;
    }
}