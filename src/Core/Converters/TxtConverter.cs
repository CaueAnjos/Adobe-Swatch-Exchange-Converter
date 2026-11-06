using System.Text;

namespace ASE_Converter.Cli.Core.Converters;

/// <summary>Plain text: "Name: R,G,B" per line.</summary>
public sealed class TxtConverter : IPaletteConverter
{
    public string Key => "txt";
    public string Extension => ".txt";

    public void Write(Palette palette, Stream output)
    {
        using var writer = new StreamWriter(output, new UTF8Encoding(false), leaveOpen: true)
        {
            NewLine = "\n",
        };

        writer.WriteLine($";Palette Name: {palette.Name} -- {palette.Colors.Count} colors.");
        foreach (var c in palette.Colors)
            writer.WriteLine($"{c.Name}: {c.R},{c.G},{c.B}");
    }
}
