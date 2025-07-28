using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.CatChast.Auth;
using Medoz.CatChast.Messaging;
using Microsoft.Extensions.Logging;

using Message = Medoz.CatChast.Messaging.Message;
namespace Medoz.KoeKan.Command;

public class TwitchCommand_Start : ICommand
{
    public string CommandName => "start";

    public string HelpText => "start twitch client";

    private readonly IClientService _clientService;
    private readonly IConfigService _configService;
    private readonly IAsyncEventBus _asyncEventBus;
    private readonly ILogger _logger;

    public TwitchCommand_Start(
        IClientService clientService,
        IConfigService configService,
        IAsyncEventBus asyncEventBus,
        ILogger logger)
    {
        _configService = configService;
        _clientService = clientService;
        _asyncEventBus = asyncEventBus;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        // FIXME: TwitchClientの生成プロセスが複雑なので修正する
        var config = _configService.GetConfig();
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
            _logger.LogError("Failed to get Twitch OAuth token.");
            return;
        }

        var channels = twitchClientConfig.TryGetValue<string[]>("channels", out var channelList)
            ? channelList
            : Array.Empty<string>();

        var twitchClient = _clientService.GetOrCreateClient<TwitchTextClient>(
            new TwitchOptions() { Token = token.AccessToken, Channels = channels ?? Array.Empty<string>() },
            "twitch",
            async message =>
            {
                try
                {
                    await _asyncEventBus.PublishAsync(new Message(
                        "twitch",
                        "twitch",
                        message.Channel,
                        message.Username,
                        message.Content,
                        message.Timestamp,
                        message.IconSource
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while publishing message from Twitch client.");
                }
            });

        _ = Task.Run(async () =>
        {
            try
            {
                await twitchClient.RunAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Twitch client crashed unexpectedly.");
            }
        });
        await Task.CompletedTask;
    }
}
