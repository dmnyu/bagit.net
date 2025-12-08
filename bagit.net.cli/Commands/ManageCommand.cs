using bagit.net.cli.lib;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;


namespace bagit.net.cli.Commands;

class ManageCommand : Command<ManageCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--add")]
        public string? Add { get; set; }

        [CommandOption("--set")]
        public string? Set { get; set; }

        [CommandOption("--delete")]
        public string? Delete { get; set; }

        [CommandOption("--view")]
        public bool View { get; set; }

        [CommandArgument(0, "[directory]")]
        [Description("Path to the directory to bag.")]

        required public string Directory { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var serviceProvider = ServiceConfigurator.BuildServiceProvider<TagManager>();
        var manager = serviceProvider.GetRequiredService<TagManager>();
        if(settings.Add != null)
            manager.Add(settings.Directory, settings.Add);

        if (settings.Set != null)
            manager.Set(settings.Directory, settings.Set);

        if (settings.Delete != null)
            manager.Delete(settings.Directory, settings.Delete);

        if (settings.View)
        {
            manager.View(settings.Directory);
        }
        

        manager.LogMessages();
        return 0;
    }
}


