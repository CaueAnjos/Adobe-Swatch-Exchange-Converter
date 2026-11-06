using System.Buffers.Binary;
using System.Text;

namespace ASE_Converter.Cli.Core;

/// <summary>Reads Adobe Swatch Exchange (.ase) files. All values are big-endian.</summary>
public static class AseReader
{
    private const int HeaderSize = 12;
    private const int BlockHeaderSize = 6;
    private const ushort ColorEntryBlock = 0x0001; // group start/end (0xC001/0xC002) are ignored

    public static Palette Read(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        var (colors, skipped) = Parse(data);
        return new Palette(Path.GetFileNameWithoutExtension(path), colors, skipped);
    }

    public static (List<PaletteColor> Colors, int Skipped) Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderSize || !data[..4].SequenceEqual("ASEF"u8))
            throw new InvalidDataException("Not an ASE file (missing 'ASEF' signature).");

        // data[4..8] = major/minor version, unused.
        int blockCount = BinaryPrimitives.ReadInt32BigEndian(data[8..12]);

        var colors = new List<PaletteColor>();
        int skipped = 0;
        int pos = HeaderSize;

        for (int i = 0; i < blockCount; i++)
        {
            if (pos + BlockHeaderSize > data.Length)
                throw new InvalidDataException(
                    "Unexpected end of file while reading block header."
                );

            ushort blockId = BinaryPrimitives.ReadUInt16BigEndian(data[pos..]);
            int blockLength = BinaryPrimitives.ReadInt32BigEndian(data[(pos + 2)..]);
            pos += BlockHeaderSize;

            if (blockLength < 0 || pos + blockLength > data.Length)
                throw new InvalidDataException("Block length exceeds file size.");

            if (blockId == ColorEntryBlock)
            {
                var color = ParseColor(data.Slice(pos, blockLength));
                if (color is null)
                    skipped++;
                else
                    colors.Add(color);
            }

            pos += blockLength;
        }

        return (colors, skipped);
    }

    private static PaletteColor? ParseColor(ReadOnlySpan<byte> block)
    {
        if (block.Length < 2)
            throw new InvalidDataException("Color block too short.");

        int nameChars = BinaryPrimitives.ReadUInt16BigEndian(block); // includes null terminator
        int pos = 2;

        if (block.Length < pos + nameChars * 2 + 4)
            throw new InvalidDataException("Color block too short.");

        string name = Encoding
            .BigEndianUnicode.GetString(block.Slice(pos, nameChars * 2))
            .TrimEnd('\0');
        pos += nameChars * 2;

        string model = Encoding.ASCII.GetString(block.Slice(pos, 4)).TrimEnd();
        pos += 4;

        int components = model switch
        {
            "RGB" => 3,
            "CMYK" => 4,
            "Gray" => 1,
            _ => 0,
        };
        if (components == 0)
            return null;

        if (block.Length < pos + components * 4)
            throw new InvalidDataException("Color block too short.");

        return model switch
        {
            "RGB" => new PaletteColor(
                name,
                ReadFloat(block, pos, 0),
                ReadFloat(block, pos, 1),
                ReadFloat(block, pos, 2)
            ),
            "Gray" => GrayColor(name, ReadFloat(block, pos, 0)),
            _ => FromCmyk(
                name,
                ReadFloat(block, pos, 0),
                ReadFloat(block, pos, 1),
                ReadFloat(block, pos, 2),
                ReadFloat(block, pos, 3)
            ),
        };
    }

    private static float ReadFloat(ReadOnlySpan<byte> block, int start, int index) =>
        BinaryPrimitives.ReadSingleBigEndian(block[(start + index * 4)..]);

    private static PaletteColor GrayColor(string name, float g) => new(name, g, g, g);

    private static PaletteColor FromCmyk(string name, float c, float m, float y, float k) =>
        new(name, (1 - c) * (1 - k), (1 - m) * (1 - k), (1 - y) * (1 - k));
}
