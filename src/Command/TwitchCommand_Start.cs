using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Annotations;
using System.Windows.Input;

using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Microsoft.VisualBasic;
using Medoz.CatChast.Auth;
namespace Medoz.KoeKan.Command;

public class TwitchCommand_Start : ICommand
{
    public string CommandName => "start";

    public string HelpText => "start twitch client";

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
        var listener = _listenerService.GetListener();
        var twitchClientConfig = config.Clients.TryGetValue("twitch", out var clientConfig)
            ? clientConfig
            : new DynamicConfig();

        var clientId = twitchClientConfig.TryGetValue<string>("clientId", out var id)
            ? id
            : null;

        var oauth = new TwitchOAuthWithImplicit(new TwitchOAuthOptions(clientId ?? "", 53919));
        var token = await oauth.AuthorizeAsync();
        if (string.IsNullOrEmpty(token.AccessToken))
        {
            _listenerService.AddLogMessage("Failed to get Twitch OAuth token.");
            return;
        }

        var channels = twitchClientConfig.TryGetValue<string[]>("channels", out var channelList)
            ? channelList
            : Array.Empty<string>();

        TwitchTextClient? twitchClient = null;
        if (!_clientService.TryGetClient("twitch", out var existingClient))
        {
            twitchClient = new TwitchTextClient(new TwitchOptions(){Token = token.AccessToken, Channels = channels ?? new string[] { } });

            twitchClient.OnReceiveMessage += message => {
                listener?.AddMessage(message);
                return Task.CompletedTask;
            };

            listener?.AddMessageConverter(
                nameof(TwitchTextClient),
                (message) => new ChatMessage(
                    ChatMessageType.Twitch,
                    message.Channel,
                    message.IconSource,
                    message.Username,
                    message.Content,
                    message.Timestamp,
                    false));
            _clientService.RegisterClient("twitch", twitchClient);
        }
        if (twitchClient is null)
        {
            if (existingClient is TwitchTextClient)
            {
                twitchClient = existingClient as TwitchTextClient;
            }
            else
            {
                _listenerService.AddLogMessage("Twitch client is not a valid TwitchClient instance.");
                return;
            }
        }

        twitchClient.OnReady += async () => {
            _listenerService.AddLogMessage("Twitch is ready.");
            await Task.CompletedTask;
        };
        _configService.SaveSecret();
        _ = Task.Run(() => twitchClient.RunAsync());
        await Task.CompletedTask;
    }
}
