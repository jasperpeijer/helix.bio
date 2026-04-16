namespace Helix.Bio;

public interface ISequence
{
    int Length { get; }
    int GetMemoryFootprintBytes();
    string ToString();
    
    char this[int index] { get; }
}