using ASE_Converter.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<ConvertCommand>();

app.Configure(config =>
{
    config.SetApplicationName("ase-convert");
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
    config.AddExample("palettes/");
    config.AddExample("a.ase", "b.ase", "-f", "txt", "-f", "hex");
    config.AddExample("palettes/", "-o", "out/", "-f", "aco");
});

return app.Run(args);
