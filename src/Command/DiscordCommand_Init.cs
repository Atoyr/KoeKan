using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Options;
namespace Medoz.KoeKan.Command;

public class DiscordCommand_Init : ICommand
{
    public string CommandName => "init";
    public string HelpText => "init discord client setting.";

    private readonly IListenerService _listenerService;
    private readonly IConfigService _configService;
    private readonly string _clientConfigName = "discord";

    public DiscordCommand_Init(
        IListenerService listenerService,
        IClientService clientService,
        IConfigService configService)
    {
        _listenerService = listenerService;
        _configService = configService;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length <= 1;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        string? clientName = null;
        if (args.Length > 0)
        {
            clientName = args[0];
        }
        var config = _configService.GetConfig();
        DynamicConfig? clientConfig;
        config.Clients.TryGetValue(_clientConfigName, out clientConfig);
        if (clientConfig is null)
        {
            clientConfig = new DynamicConfig();
            config.Clients[_clientConfigName] = clientConfig;
        }

        if (clientConfig.TryGetValue("default_channel_id", out ulong defaultChannelId) == false)
        {
            clientConfig["default_channel_id"] = 0;
        }
        _configService.SaveConfig();
        _listenerService.AddLogMessage($"Initialze discord");
        await Task.CompletedTask;
    }
}

