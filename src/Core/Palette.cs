namespace ASE_Converter.Cli.Core;

public sealed record PaletteColor(string Name, float Red, float Green, float Blue)
{
    public byte R => ToByte(Red);
    public byte G => ToByte(Green);
    public byte B => ToByte(Blue);

    private static byte ToByte(float v) =>
        (byte)MathF.Round(Math.Clamp(v, 0f, 1f) * 255f, MidpointRounding.AwayFromZero);
}

/// <param name="Name">File stem of the source ASE file.</param>
/// <param name="Colors">Colors in file order.</param>
/// <param name="SkippedCount">Color entries in a color model we can't turn into RGB (e.g. LAB).</param>
public sealed record Palette(string Name, IReadOnlyList<PaletteColor> Colors, int SkippedCount = 0);
