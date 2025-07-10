using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Annotations;
using System.Windows.Input;

using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Microsoft.VisualBasic;
namespace Medoz.KoeKan.Command;

public class TwitchCommand_Start : ICommand
{
    public string CommandName => "start";

    public string HelpText => "start discord client";

    private readonly IListenerService _listenerService;
    private readonly IClientService _clientService;
    private readonly IConfigService _configService;

    public TwitchCommand_Start(
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
        var token = secret.GetValue("twitch.token");
        if (string.IsNullOrEmpty(token))
        {
            _listenerService.AddLogMessage("Twitch token is not set.");
            return;
        }

        var twitchClientConfig = config.Clients["twitch"];
        var channels = twitchClientConfig.GetValue<string[]>("channels");
        var twitchClient = new TwitchClient(new TwitchOptions(){Token = token, Channels = channels ?? new string[] { } });
        twitchClient.OnReceiveMessage += (message) => {
            _listenerService.AddMessage(message);
            return Task.CompletedTask;
        };
        twitchClient.OnReady += async () => {
            _listenerService.AddLogMessage("Twitch is ready.");
            await Task.CompletedTask;
        };
        token = await twitchClient.AuthAsync();
        secret.SetValue("twitch.token", token);
        _configService.SaveSecret();
        _ = Task.Run(() => twitchClient.RunAsync());
        await Task.CompletedTask;
    }
}


