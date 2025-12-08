using bagit.net.cli.Commands;
using bagit.net.domain;
using Spectre.Console;
using Spectre.Console.Cli;

namespace bagit.net.cli;

class BagitCLI
{
    public static CommandApp app;

    static BagitCLI() {
        app = new CommandApp();
    }

    static int Main(string[] args)
    {
        
        app.SetDefaultCommand<CreateCommand>();

        if (args == null || args.Length == 0)
        {
            return new HelpCommand().Execute(null, new HelpCommand.Settings(), new CancellationToken());
        }

        foreach (string arg in args) { 
            if(arg == "--help" || arg == "-h")
            {
                return new HelpCommand().Execute(null, new HelpCommand.Settings(), new CancellationToken());

            }
        }

        if (args[0] == "--version")
        {
            AnsiConsole.MarkupLine($"bagit.net v{Bagit.VERSION}");
            return 0;
        }


        app.Configure(config =>
        {
            config.AddCommand<CreateCommand>("create")
                 .WithDescription("Create a BagIt bag from a directory");
            config.AddCommand<HelpCommand>("help")
                .WithDescription("Display the help page");
            config.AddCommand<ValidateCommand>("validate")
                .WithDescription("Validate a BagIt bag");
            config.AddCommand<ManageCommand>("manage")
                .WithDescription("Manage metadata tags");
        });

        return app.Run(args);
    }

}