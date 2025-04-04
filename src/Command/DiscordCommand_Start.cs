using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Annotations;
using System.Windows.Input;

using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Microsoft.VisualBasic;
namespace Medoz.KoeKan.Command;

public class DiscordCommand_Start : ICommand
{
    public string CommandName => "start";

    public string HelpText => "start discord client";

    private readonly IListenerService _listenerService;
    private readonly IClientService _clientService;
    private readonly IConfigService _configService;

    public DiscordCommand_Start(
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
        var config = _configService.GetConfig();
        var secret = _configService.GetSecret();
        var token = secret.GetValue("discord.token");
        if (string.IsNullOrEmpty(token))
        {
            _listenerService.AddLogMessage("Discord token is not set.");
            return;
        }

        var discordClientConfig = config.Clients["discord"];
        var discordClient = new DiscordClient(new DiscordOptions(){Token = token });
        discordClient.OnReceiveMessage += (message) => {
            _listenerService.AddMessage(message);
            return Task.CompletedTask;
        };
        discordClient.OnReady += async () => {
            if(discordClientConfig.TryGetValue("defalut_channel_id", out ulong channelId))
            {
                discordClient.SetChannel(channelId);
            }
            _listenerService.AddLogMessage("Discord is ready.");
            await Task.CompletedTask;
        };
        await discordClient.AuthAsync();
        _ = Task.Run(() => discordClient.RunAsync());
        await Task.CompletedTask;
    }
}

