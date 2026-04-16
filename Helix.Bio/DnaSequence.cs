using System.Buffers;
using System.Text;

namespace Helix.Bio;

public class DnaSequence : ISequence
{
    private readonly byte[] _packedData;
    private readonly int[] _compositionMap;
    
    public int Length { get; }
    public double GcContent { get; }

    public DnaSequence(ReadOnlySpan<char> sequence)
    {
        Length = sequence.Length;
        int byteCount = (Length + 1) / 2;
        _packedData = new byte[byteCount];
        _compositionMap = new int[256];
        
        for (var i = 0; i < Length; i++)
        {
            char currentBase = sequence[i];
            char upperBase = char.ToUpper(currentBase);
            
            byte nibble = EncodeIUPAC(currentBase);
            int byteIndex = i / 2;
            var bitOffset = (i % 2) * 4;
            _packedData[byteIndex] |= (byte)(nibble << bitOffset);

            if (upperBase < 256)
            {
                _compositionMap[upperBase]++;
            }
        }

        int validBases = Length - GetCount('-') - GetCount('N') - GetCount('X');
        int gcCount = GetCount('G') + GetCount('C') + GetCount('S');

        GcContent = validBases == 0 ? 0 : ((double)gcCount / validBases);
    }

    public int GetCount(char c)
    {
        char upperBase = char.ToUpper(c);
        _ = EncodeIUPAC(upperBase);
        
        return _compositionMap[upperBase];
    }

    private static byte EncodeIUPAC(char c) => char.ToUpper(c) switch
    {
        'A' => 1, 'C' => 2, 'G' => 3, 'T' => 4,
        'U' => 4, // Treat U as T in DNA context
        'R' => 5, 'Y' => 6, 'S' => 7, 'W' => 8,
        'K' => 9, 'M' => 10, 'B' => 11, 'D' => 12,
        'H' => 13, 'V' => 14, 'N' => 15, 'X' => 15, '-' => 0,
        _ => throw new ArgumentException($"Invalid IUPAC character: {c}")
    };

    private static char DecodeIUPAC(byte b) => b switch
    {
        1 => 'A', 2 => 'C', 3 => 'G', 4 => 'T',
        5 => 'R', 6 => 'Y', 7 => 'S', 8 => 'W',
        9 => 'K', 10 => 'M', 11 => 'B', 12 => 'D',
        13 => 'H', 14 => 'V', 15 => 'N', 0 => '-',
        _ => '?'
    };
    
    public int GetMemoryFootprintBytes() => _packedData.Length;

    /// <summary>
    /// Generates a new DnaSequence representing the reverse complement of the current sequence.
    /// </summary>
    public DnaSequence ReverseComplement()
    {
        const int StackAllocThreshold = 1024;
        char[]? rentedArray = null;
        
        Span<char> rcBuffer = Length <= StackAllocThreshold
            ? stackalloc char[Length]
            : (rentedArray = ArrayPool<char>.Shared.Rent(Length));

        try
        {
            for (var i = 0; i < Length; i++)
            {
                rcBuffer[i] = GetComplementBase(this[Length - 1 - i]);
            }

            return new DnaSequence(rcBuffer[..Length]);
        }
        finally
        {
            if (rentedArray != null) ArrayPool<char>.Shared.Return(rentedArray);
        }
    }
    
    /// <summary>
    /// Returns the strict biological complement for any IUPAC character.
    /// </summary>
    private static char GetComplementBase(char c) => char.ToUpper(c) switch
    {
        'A' => 'T', 'T' => 'A', 'U' => 'A',
        'C' => 'G', 'G' => 'C',
        // Ambiguity codes also have strict biological complements!
        'R' => 'Y', 'Y' => 'R', // Purine <-> Pyrimidine
        'S' => 'S', 'W' => 'W', // Strong/Weak bind to themselves
        'K' => 'M', 'M' => 'K', // Keto <-> Amino
        'B' => 'V', 'V' => 'B',
        'D' => 'H', 'H' => 'D',
        'N' => 'N', 'X' => 'X', '-' => '-',
        _ => '?'
    };

    public char this[int index]
    {
        get
        {
            if (index < 0 || index >= Length) throw new IndexOutOfRangeException();

            int byteIndex = index / 2;
            int bitOffset = (index % 2) * 4;
            int nibble = (_packedData[byteIndex] >> bitOffset) & 0b1111;

            return DecodeIUPAC((byte)nibble);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder(Length);

        for (var i = 0; i < Length; i++) sb.Append(this[i]);

        return sb.ToString();
    }

}