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

public class DiscordCommand_Init : ICommand
{
    public string CommandName => "init";
    public string HelpText => "init discord client setting.";

    private readonly IConfigService _configService;
    private readonly ILogger _logger;
    private readonly string _clientConfigName = "discord";

    public DiscordCommand_Init(
        IConfigService clientService,
        ILogger logger)
    {
        _configService = clientService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public Task ExecuteCommandAsync(string[] args)
    {
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
        _logger.LogInformation("Initialize discord");
        return Task.CompletedTask;
    }
}

