using System.Buffers.Binary;

namespace ASE_Converter.Cli.Core.Converters;

/// <summary>Adobe Color Swatch, version 1 (RGB only, no names).</summary>
public sealed class AcoConverter : IPaletteConverter
{
    public string Key => "aco";
    public string Extension => ".aco";

    public void Write(Palette palette, Stream output)
    {
        if (palette.Colors.Count > ushort.MaxValue)
            throw new InvalidOperationException("ACO files can hold at most 65535 colors.");

        // header: version (2) + count (2); per color: space (2) + 4 x ushort (8)
        var buffer = new byte[4 + palette.Colors.Count * 10];
        var span = buffer.AsSpan();

        BinaryPrimitives.WriteUInt16BigEndian(span, 1);
        BinaryPrimitives.WriteUInt16BigEndian(span[2..], (ushort)palette.Colors.Count);

        int pos = 4;
        foreach (var c in palette.Colors)
        {
            BinaryPrimitives.WriteUInt16BigEndian(span[pos..], 0); // color space 0 = RGB
            BinaryPrimitives.WriteUInt16BigEndian(span[(pos + 2)..], (ushort)(c.R * 257)); // 8 -> 16 bit
            BinaryPrimitives.WriteUInt16BigEndian(span[(pos + 4)..], (ushort)(c.G * 257));
            BinaryPrimitives.WriteUInt16BigEndian(span[(pos + 6)..], (ushort)(c.B * 257));
            BinaryPrimitives.WriteUInt16BigEndian(span[(pos + 8)..], 0);
            pos += 10;
        }

        output.Write(buffer);
    }
}
