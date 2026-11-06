namespace ASE_Converter.Cli.Core.Converters;

public interface IPaletteConverter
{
    /// <summary>Name used on the command line (e.g. "aco").</summary>
    string Key { get; }

    /// <summary>Output file extension including the dot (e.g. ".aco").</summary>
    string Extension { get; }

    void Write(Palette palette, Stream output);
}
