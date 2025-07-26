using System.Text;

using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Microsoft.VisualBasic;

using Medoz.CatChast.Messaging;
using Message = Medoz.CatChast.Messaging.Message;
using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class DiscordCommand_Guilds : ICommand
{
    public string CommandName => "guilds";

    public string HelpText => "get usable discord guilds";

    private readonly IClientService _clientService;

    private readonly ILogger _logger;

    public DiscordCommand_Guilds(
        IClientService clientService,
        ILogger logger)
    {
        _clientService = clientService;
        _logger = logger;
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
            _logger.LogError("Discord client is not started.");
            return;
        }
        StringBuilder sb = new();
        foreach(var g in await discordClient.GetGuildsAsync())
        {
            sb.AppendLine($"{g.Id} : {g.Name}");
        }
        _logger.LogInformation(sb.ToString());
    }
}



