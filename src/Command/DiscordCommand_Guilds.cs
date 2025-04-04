using System.Text;

using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Microsoft.VisualBasic;
namespace Medoz.KoeKan.Command;

public class DiscordCommand_Guilds : ICommand
{
    public string CommandName => "guilds";

    public string HelpText => "get usable discord guilds";

    private readonly IListenerService _listenerService;
    private readonly IClientService _clientService;
    private readonly IConfigService _configService;

    public DiscordCommand_Guilds(
        IListenerService listenerService,
        IClientService clientService,
        IConfigService configService)
    {
        _listenerService = listenerService;
        _configService = configService;
        _clientService = clientService;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        var client = _clientService.GetClient("discord");
        if (client is not DiscordClient discordClient)
        {
            _listenerService.AddLogMessage("Discord client is not started.");
            return;
        }
        StringBuilder sb = new();
        foreach(var g in await discordClient.GetGuildsAsync())
        {
            sb.AppendLine($"{g.Id} : {g.Name}");
        }
        _listenerService.AddCommandMessage(sb.ToString());
        await Task.CompletedTask;
    }
}



