using Spectre.Console;
using Spectre.Console.Cli;
using bagit.net.cli.Commands;

namespace bagit.net.cli;

class BagitCLI
{
    const string VERSION = "0.1.0";
    static int Main(string[] args)
    {
        var app = new CommandApp();
        app.SetDefaultCommand<BagCommand>();

        app.Configure(config =>
        {
            config.AddCommand<ValidateCommand>("validate")
                .WithDescription("Validate an existing BagIt bag.");
        });

        if (args.Length == 1 && args[0] == "--version")
        {
            AnsiConsole.MarkupLine($"[green]bagit.net v{VERSION}[/]");
            return 0;
        }

        return app.Run(args);
    }
}