using System.Text;

using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Microsoft.VisualBasic;
namespace Medoz.KoeKan.Command;

public class DiscordCommand_Channels : ICommand
{
    public string CommandName => "channels";

    public string HelpText => "get usable discord channels";

    private readonly IListenerService _listenerService;
    private readonly IClientService _clientService;
    private readonly IConfigService _configService;

    public DiscordCommand_Channels(
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
        foreach(var c in discordClient.GetChannels())
        {
            sb.AppendLine($"{c.GuildName} | {c.Id} : {c.Name}");
        }
        _listenerService.AddCommandMessage(sb.ToString());
        await Task.CompletedTask;
    }
}


