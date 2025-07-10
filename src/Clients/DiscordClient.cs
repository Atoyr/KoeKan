using Microsoft.Extensions.Logging;

using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace Medoz.KoeKan.Clients;

public class DiscordClient: ITextClient
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new ();

    private IMessageChannel? _messageChannel;

    public string Name => GetType().Name;
    public bool IsRunning => _client.ConnectionState == ConnectionState.Connected;

    public event Func<ClientMessage, Task>? OnReceiveMessage;
    public event Func<Task>? OnReady;

    public DiscordClient(DiscordOptions options)
    {
        _options = options;

        _client = new DiscordSocketClient(new DiscordSocketConfig(){ GatewayIntents = GatewayIntents.All});
        _client.MessageReceived += MessageReceivedAsync;
        _client.Ready += ReadyAsync;
    }

    public async Task RunAsync()
    {
        if (_client.ConnectionState == ConnectionState.Connected)
        {
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

    public async Task SendMessageAsync(ClientMessage message)
    {
        if (_messageChannel is not null)
        {
            await _messageChannel.SendMessageAsync(message.Content);
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
                    new ClientMessage(
                        Name,
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

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public Task<string> AuthAsync()
    {
        if (_options.Token is null)
        {
            throw new InvalidOperationException("Failed to create an instance of the client.");
        }
        // Tokenの検証
        TokenUtils.ValidateToken(TokenType.Bot, _options.Token);
        return Task.FromResult(_options.Token);
    }
}