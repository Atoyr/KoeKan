using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Options;

using Medoz.CatChast.Messaging;
using Message = Medoz.CatChast.Messaging.Message;
using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class DiscordCommand_Set : ICommand
{
    public string CommandName => "set";
    public string HelpText => "set discord property";

    private readonly IClientService _clientService;
    private readonly IConfigService _configService;

    private readonly ILogger _logger;
    private readonly string _clientConfigName = "discord";

    public DiscordCommand_Set(
        IClientService clientService,
        IConfigService configService,
        ILogger logger)
    {
        _clientService = clientService;
        _configService = configService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 2;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (!CanExecute(args))
        {
            _logger.LogError("Invalid arguments for set command.");
            return;
        }
        var config = _configService.GetConfig();
        if (!config.Clients.TryGetValue(_clientConfigName, out DynamicConfig? clientConfig))
        {
            _logger.LogError($"Client config {_clientConfigName} not found.");
            return;
        }

        switch(args[0])
        {
            case "defaultChannel":
                SetDefaultChannelId(clientConfig, args[1]);
                break;
            case "token":
                SetToken(args[1]);
                break;
            case "channel":
                SetChannel(args[1]);
                break;
            default:
                _logger.LogError($"Unknown property {args[0]}.");
                return;
        }
        await Task.CompletedTask;
    }

    private void SetDefaultChannelId(DynamicConfig clientConfig, string id)
    {
        if (ulong.TryParse(id, out ulong channelId) == false)
        {
            _logger.LogError($"Invalid channel id {id}.");
            return;
        }
        clientConfig["default_channel_id"] = channelId;
        _configService.SaveConfig();
        _logger.LogInformation($"Default channel set to {channelId}.");
    }

    private void SetToken(string token)
    {
        var secret = _configService.GetSecret();
        secret.SetValue("discord.token", token);
        _configService.SaveSecret();
        _logger.LogInformation($"Token set to {token}.");
    }

    private void SetChannel(string id)
    {
        if (ulong.TryParse(id, out ulong channelId) == false)
        {
            _logger.LogInformation($"Invalid channel id {id}.");
            return;
        }
        var client = _clientService.GetClient(_clientConfigName);
        if (client is not DiscordClient discordClient)
        {
            _logger.LogError("Discord client is not started.");
            return;
        }
        discordClient.SetChannel(channelId);
    }
}