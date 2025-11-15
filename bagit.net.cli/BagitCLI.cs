using Spectre.Console;
using Spectre.Console.Cli;
using bagit.net.cli.Commands;

namespace bagit.net.cli;

class BagitCLI
{
    static int Main(string[] args)
    {
        var app = new CommandApp();
        app.SetDefaultCommand<BagCommand>();

        if (args[0] == "--version")
        {
            AnsiConsole.MarkupLine($"[green]bagit.net {Bagit.VERSION}[/]");
            return 0;
        }

        app.Configure(config =>
        {
            config.AddCommand<BagCommand>("create")
                  .WithDescription("Create a BagIt bag from a directory.");
        });

        return app.Run(args);
    }
}