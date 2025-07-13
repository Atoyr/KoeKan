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

    public string HelpText => "start tiwitch client";

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
        // FIXME: TwitchClientの生成プロセスが複雑なので修正する
        var config = _configService.GetConfig();
        var secret = _configService.GetSecret();
        var token = secret.GetValue("twitch.token");

        var twitchClientConfig = config.Clients.TryGetValue("twitch", out var clientConfig)
            ? clientConfig
            : new DynamicConfig();
        var channels = twitchClientConfig.TryGetValue<string[]>("channels", out var channelList)
            ? channelList
            : Array.Empty<string>();


        TwitchClient? twitchClient = null;
        if (!_clientService.TryGetClient("twitch", out var existingClient))
        {
            twitchClient = new TwitchClient(new TwitchOptions(){Token = token, Channels = channels ?? new string[] { } });
            _clientService.RegisterClient("twitch", twitchClient);
        }
        if (twitchClient is null)
        {
            if (existingClient is TwitchClient)
            {
                twitchClient = existingClient as TwitchClient;
            }
            else
            {
                _listenerService.AddLogMessage("Twitch client is not a valid TwitchClient instance.");
                return;
            }
        }
        twitchClient!.OnReceiveMessage += (message) => {
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
