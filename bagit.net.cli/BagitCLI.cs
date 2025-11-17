using bagit.net.cli.Commands;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace bagit.net.cli;

class BagitCLI
{
    static int Main(string[] args)
    {
        Bagit.InitLogger();
        Bagit.Logger.LogInformation("Using bagit.net v{Version}", Bagit.VERSION);

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