using System.ComponentModel;
using ASE_Converter.Cli.Core;
using ASE_Converter.Cli.Core.Converters;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ASE_Converter.Cli.Commands;

internal sealed class ConvertCommand : Command<ConvertCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<paths>")]
        [Description("One or more .ase files and/or folders containing .ase files")]
        public string[] Paths { get; init; } = [];

        [CommandOption("-f|--format")]
        [Description("Output format: aco, txt or hex. Repeat for several (default: aco)")]
        public string[] Formats { get; init; } = [];

        [CommandOption("-o|--output")]
        [Description("Output folder (default: next to each source file)")]
        public string? OutputDirectory { get; init; }

        public override ValidationResult Validate()
        {
            foreach (var path in Paths)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                    return ValidationResult.Error($"Path does not exist: {path}");
            }

            foreach (var format in Formats)
            {
                if (!ConverterRegistry.TryGet(format, out _))
                {
                    return ValidationResult.Error(
                        $"Unknown format '{format}'. Available: {string.Join(", ", ConverterRegistry.Keys)}"
                    );
                }
            }

            return ValidationResult.Success();
        }
    }

    public override int Execute(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        string[] keys = settings.Formats.Length > 0 ? settings.Formats : ["aco"];
        var converters = keys.Select(k =>
            {
                ConverterRegistry.TryGet(k, out var c);
                return c;
            })
            .DistinctBy(c => c.Key)
            .ToList();

        var files = ResolveInputs(settings.Paths).ToList();
        if (files.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No ASE files found.[/]");
            return 1;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumns("File", "Format", "Colors", "Output");
        int failures = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Palette palette;
            try
            {
                palette = AseReader.Read(file);
            }
            catch (Exception ex)
                when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Failed to read[/] {file}: {ex.Message}");
                failures++;
                continue;
            }

            if (palette.SkippedCount > 0)
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[yellow]{palette.Name}: skipped {palette.SkippedCount} color(s) in an unsupported color model (e.g. LAB).[/]"
                );
            }

            string outDir =
                settings.OutputDirectory ?? Path.GetDirectoryName(Path.GetFullPath(file))!;
            Directory.CreateDirectory(outDir);

            foreach (var converter in converters)
            {
                string outPath = Path.Combine(outDir, palette.Name + converter.Extension);
                try
                {
                    using var stream = File.Create(outPath);
                    converter.Write(palette, stream);
                    table.AddRow(
                        Markup.Escape(palette.Name),
                        converter.Key.ToUpperInvariant(),
                        palette.Colors.Count.ToString(),
                        Markup.Escape(outPath)
                    );
                }
                catch (Exception ex)
                    when (ex
                            is IOException
                                or InvalidOperationException
                                or UnauthorizedAccessException
                    )
                {
                    AnsiConsole.MarkupLineInterpolated(
                        $"[red]Failed to write[/] {outPath}: {ex.Message}"
                    );
                    failures++;
                }
            }
        }

        AnsiConsole.Write(table);
        return failures == 0 ? 0 : 1;
    }

    private static IEnumerable<string> ResolveInputs(IEnumerable<string> paths)
    {
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };

        return paths
            .SelectMany(p =>
                Directory.Exists(p) ? Directory.EnumerateFiles(p, "*.ase", options) : [p]
            )
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
