using System.Text;

namespace ASE_Converter.Cli.Core.Converters;

/// <summary>Hex list: "#RRGGBB" per line.</summary>
public sealed class HexConverter : IPaletteConverter
{
    public string Key => "hex";
    public string Extension => ".hex";

    public void Write(Palette palette, Stream output)
    {
        using var writer = new StreamWriter(output, new UTF8Encoding(false), leaveOpen: true)
        {
            NewLine = "\n",
        };

        writer.WriteLine($";Palette Name: {palette.Name} -- {palette.Colors.Count} colors.");
        foreach (var c in palette.Colors)
            writer.WriteLine($"#{c.R:X2}{c.G:X2}{c.B:X2}");
    }
}
