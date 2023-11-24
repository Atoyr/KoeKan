using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Discord;
using Discord.WebSocket;

namespace Medoz.TextTransporter.Client;

public class Discord: IClient
{
    private string _source = "Discord";
    private DiscordSocketClient _client;
    private DiscordOptions _options;

    protected ILogger? Logger { get; set; }

    public event Func<Message, Task>? OnReceiveMessage;

    public Discord(DiscordOptions options, ILogger? logger)
    {
        _options = options;
        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;
    }

    public async Task RunAsync(CancellationToken token)
    {
        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();
        await Task.Delay(-1, token);
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        // ボット自身のメッセージは無視
        if (message.Author.Id == _client.CurrentUser.Id) return;

        if (OnReceiveMessage is not null)
        {
            await OnReceiveMessage.Invoke(new Message(_source, message.Channel.Name, message.Author.Username, message.Content));
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    public void Dispose()
    {

    }
}