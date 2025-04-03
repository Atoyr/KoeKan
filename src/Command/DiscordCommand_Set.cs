using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Options;
namespace Medoz.KoeKan.Command;

public class DiscordCommand_Set : ICommand
{
    public string CommandName => "set";
    public string HelpText => "set discord property";

    private readonly IListenerService _listenerService;
    private readonly IConfigService _configService;
    private readonly string _clientConfigName = "discord";

    public DiscordCommand_Set(
        IListenerService listenerService,
        IConfigService configService)
    {
        _listenerService = listenerService;
        _configService = configService;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 2;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (!CanExecute(args))
        {
            _listenerService.AddLogMessage("Invalid arguments for set command.");
            return;
        }
        var config = _configService.GetConfig();
        if (!config.Clients.TryGetValue(_clientConfigName, out DynamicConfig? clientConfig))
        {
            _listenerService.AddLogMessage($"Client config {_clientConfigName} not found.");
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
            default:
                _listenerService.AddLogMessage($"Unknown property {args[0]}.");
                return;
        }
        await Task.CompletedTask;
    }

    private void SetDefaultChannelId(DynamicConfig clientConfig, string id)
    {
        ulong channelId = 0;
        try
        {
            channelId = Convert.ToUInt64(id);
        }
        catch (Exception)
        {
            _listenerService.AddLogMessage($"Invalid channel id {id}.");
            return;
        }
        clientConfig["default_channel_id"] = channelId;
        _configService.SaveConfig();
        _listenerService.AddLogMessage($"Username set to {channelId}.");
    }

    private void SetToken(string token)
    {
        var secret = _configService.GetSecret();
        secret.SetValue("discord.token", token);
        _configService.SaveSecret();
        _listenerService.AddLogMessage($"Token set to {token}.");
    }
}


