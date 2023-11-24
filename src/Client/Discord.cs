using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Discord;
using Discord.WebSocket;

namespace Medoz.TextTransporter.Client;

public class Discord: IClient
{
    private DiscordSocketClient _client;
    private DiscordOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();

    private string _channelId = string.Empty;

    protected ILogger? Logger { get; set; }

    public event Func<Message, Task>? OnReceiveMessage;

    public Discord(DiscordOptions options, ILogger? logger)
    {
        _options = options;
        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;
    }

    public async Task RunAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();
        await Task.Delay(-1, _cancellationTokenSource.Token);
        await _client.StopAsync();
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.CompletedTask;
    }

    public void ChangeChannel()
    {

    }

    public Task<IEnumerable<DiscordGuild>> GetGuildsAsync()
        => Task.FromResult(_client.Guilds.Select(x => new DiscordGuild(x.Id.ToString(), x.Name)));

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        // ボット自身のメッセージは無視
        if (message.Author.Id == _client.CurrentUser.Id) return;

        if (OnReceiveMessage is not null)
        {
            await OnReceiveMessage.Invoke(new Message(ClientType.Discord, message.Channel.Name, message.Author.Username, message.Content));
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Logger?.LogInformation(log.ToString());
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}