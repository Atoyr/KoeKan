using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;
using Medoz.CatChast.Messaging;
using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class DiscordCommand_Start : ICommand
{
    public string CommandName => "start";

    public string HelpText => "start discord client";

    private readonly IClientService _clientService;
    private readonly IConfigService _configService;
    private readonly IAsyncEventBus _asyncEventBus;
    private readonly ILogger _logger;


    public DiscordCommand_Start(
        IClientService clientService,
        IConfigService configService,
        IAsyncEventBus asyncEventBus,
        ILogger logger)
    {
        _clientService = clientService;
        _configService = configService;
        _asyncEventBus = asyncEventBus;
        _logger = logger;
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
            _logger.LogError("Discord token is not set.");
            return;
        }

        var discordClientConfig = config.Clients["discord"];
        var option = new DiscordOptions(){Token = token };
        var discordClient = _clientService.CreateClient<DiscordClient>(option, "discord", async message =>
        {
            try
            {
                await _asyncEventBus.PublishAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while publishing message from Discord client.");
            }
        });

        discordClient.OnReady += () => {
            // FIXME: OnReadyではない場所でチャンネルを設定する必要がある
            if(discordClientConfig.TryGetValue("default_channel_id", out ulong channelId))
            {
                discordClient.SetChannel(channelId);
            }
            return Task.CompletedTask;
        };


        await discordClient.AuthAsync();
        _ = Task.Run(async () =>
        {
            try
            {
                await discordClient.RunAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Discord client crashed unexpectedly.");
            }
        });
    }
}

