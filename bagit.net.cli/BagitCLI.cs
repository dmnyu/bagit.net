using bagit.net.cli.Commands;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace bagit.net.cli;

class BagitCLI
{
    public static CommandApp app;
    static int Main(string[] args)
    {
        app = new CommandApp();
        app.SetDefaultCommand<BagCommand>();


        
        if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
        {
            return new HelpCommand().Execute(null, new HelpCommand.Settings(), new CancellationToken());
        }
        
        if (args[0] == "--version")
        {
            AnsiConsole.MarkupLine($"[green]bagit.net {Bagit.VERSION}[/]");
            return 0;
        }


        app.Configure(config =>
        {
            config.AddCommand<BagCommand>("create")
                 .WithDescription("Create a BagIt bag from a directory");
            config.AddCommand<HelpCommand>("help")
                .WithDescription("display the help page");
        });

        return app.Run(args);
    }

}