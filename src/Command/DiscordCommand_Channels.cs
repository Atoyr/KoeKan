using System.Text;

using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Microsoft.VisualBasic;
using Medoz.CatChast.Messaging;
using Message = Medoz.CatChast.Messaging.Message;
using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class DiscordCommand_Channels : ICommand
{
    public string CommandName => "channels";

    public string HelpText => "get usable discord channels";

    private readonly IClientService _clientService;

    private readonly ILogger _logger;

    public DiscordCommand_Channels(
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

    public Task ExecuteCommandAsync(string[] args)
    {
        var client = _clientService.GetClient("discord");
        if (client is not DiscordClient discordClient)
        {
            _logger.LogError("Discord client is not started.");
            return Task.CompletedTask;
        }
        StringBuilder sb = new();
        foreach (var c in discordClient.GetChannels())
        {
            sb.AppendLine($"{c.GuildName} | {c.Id} : {c.Name}");
        }
        _logger.LogInformation(sb.ToString());
        return Task.CompletedTask;
    }
}


