using Microsoft.Extensions.Logging;

using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace Medoz.KoeKan.Clients;

public class DiscordClient: ITextClient
{
    private DiscordSocketClient _client;
    private DiscordOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();

    private IMessageChannel? _messageChannel;

    private ILogger? _logger { get; set; }

    public event Func<Message, Task>? OnReceiveMessage;
    public event Func<Task>? OnReady;

    public DiscordClient(DiscordOptions options, ILogger? logger = null)
    {
        _options = options;
        _logger = logger;

        _client = new DiscordSocketClient(new DiscordSocketConfig(){ GatewayIntents = GatewayIntents.All});
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.Ready += ReadyAsync;
    }

    public async Task RunAsync()
    {
        if (_client.ConnectionState == ConnectionState.Connected)
        {
            _logger?.LogWarning("Discord client is Connected.");
            return;
        }
        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();
        await Task.Delay(-1, _cancellationTokenSource.Token);
    }

    public async Task StartAsync()
    {
        await _client.StartAsync();
    }

    public async Task StopAsync()
    {
        await _client.StopAsync();
    }

    public void SetChannel(ulong id)
    {
        var c = _client.GetChannel(id);
        if (c is not null && c is IMessageChannel mc)
        {
            _messageChannel = mc;
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_messageChannel is not null)
        {
            await _messageChannel.SendMessageAsync(message);
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

    public async Task ConnectToVoiceChannel(ulong channelId, Func<byte[], Task> voiceMessage)
    {
        var c = _client.GetChannel(channelId);
        if (c is null)
        {
            throw new Exception("voice channel not found.");
        }
        if (c is not SocketVoiceChannel svc)
        {
            throw new Exception("channel is not Voice Channel.");
        }
        try
        {
            var audioClient = await svc.ConnectAsync();

            // var audioStream = audioClient.CreateOpusStream();
            var audioStream = audioClient.CreatePCMStream(AudioApplication.Mixed);

            // データを受信するためのバッファ
            int blockSize = 3840; // 480 * 2 * 2 * 1
            byte[] audioBuffer = new byte[blockSize];
            byte[]? ret = null;
            int byteCount;
            while ((byteCount = await audioStream.ReadAsync(audioBuffer, 0, blockSize)) > 0)

            {
                // int byteCount = await audioStream.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                if (byteCount == 0)
                {
                    if (ret is not null)
                    {
                        await voiceMessage(ret);
                        ret = null;
                    }

                    continue; // データがない場合は続行
                }
                if (ret is null)
                {
                    ret = audioBuffer;
                }
                else
                {
                    ret = ret.Concat(audioBuffer).ToArray();
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        // ボット自身のメッセージは無視
        if (message.Author.Id == _client.CurrentUser.Id) return;

        if (OnReceiveMessage is not null)
        {
            await OnReceiveMessage.Invoke(
                    new Message(
                        ClientType.Discord, 
                        message.Channel.Name, 
                        message.Author.GlobalName, 
                        message.Content, 
                        message.Timestamp.DateTime.ToLocalTime(), 
                        message.Author.GetDisplayAvatarUrl()));
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
        _logger?.LogInformation(log.ToString());
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}