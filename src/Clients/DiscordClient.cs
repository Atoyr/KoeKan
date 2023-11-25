using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Discord;
using Discord.WebSocket;

namespace Medoz.MessageTransporter.Clients;

public class DiscordClient: IClient
{
    private DiscordSocketClient _client;
    private DiscordOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();

    private ulong _channelId = 0;
    private IMessageChannel? _channel;

    protected ILogger? Logger { get; set; }

    public event Func<Message, Task>? OnReceiveMessage;
    public event Func<Task>? OnReady;

    public DiscordClient(DiscordOptions options, ILogger? logger = null)
    {
        _options = options;

        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.Ready += ReadyAsync;
    }

    public async Task RunAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();
        await Task.Delay(-1, _cancellationTokenSource.Token);
    }

    public async Task StopAsync()
    {
        await _client.StopAsync();
    }

    public void SetChannel(ulong id)
    {
        var c = _client.GetChannel(id);
        if (c is not null)
        {
            _channelId = id;
            _channel = c as IMessageChannel;
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_channel is not null)
        {
            await _channel.SendMessageAsync(message);
        }
    }

    public Task<IEnumerable<DiscordGuild>> GetGuildsAsync()
        => Task.FromResult(_client.Guilds.Select(x => new DiscordGuild(x.Id.ToString(), x.Name)));

    public IEnumerable<DiscordChannel> GetChannels()
    {
        List<DiscordChannel> channels = new();
        foreach(var g in _client.Guilds)
        {
            foreach(var c in g.Channels)
            {
                channels.Add(new DiscordChannel(c.Id, c.Name, g.Id, g.Name));
            }
        }
        return channels;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        // ボット自身のメッセージは無視
        if (message.Author.Id == _client.CurrentUser.Id) return;

        if (OnReceiveMessage is not null)
        {
            await OnReceiveMessage.Invoke(new Message(ClientType.Discord, message.Channel.Name, message.Author.Username, message.Content));
        }
    }

    private async Task ReadyAsync()
    {
        if (OnReady is not null)
        {
            await OnReady.Invoke();
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