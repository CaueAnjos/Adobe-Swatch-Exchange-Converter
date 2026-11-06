namespace ASE_Converter.Cli.Core.Converters;

public static class ConverterRegistry
{
    private static readonly Dictionary<string, IPaletteConverter> Converters =
        new IPaletteConverter[]
        {
            new AcoConverter(),
            new TxtConverter(),
            new HexConverter(),
        }.ToDictionary(c => c.Key, StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<string> Keys => Converters.Keys;

    public static bool TryGet(string key, out IPaletteConverter converter) =>
        Converters.TryGetValue(key.TrimStart('-'), out converter!);
}
