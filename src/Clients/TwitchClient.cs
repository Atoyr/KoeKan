using System.Net.WebSockets;
using System.Text;

using Medoz.KoeKan.Data;

using Microsoft.Extensions.Options;

namespace Medoz.KoeKan.Clients;

public class TwitchClient: ITextClient
{
    private readonly string _webSocketUrl = "wss://irc-ws.chat.twitch.tv:443";
    private readonly ClientWebSocket _webSocket = new ClientWebSocket();

    private readonly TwitchOptions _options;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event Func<Message, Task>? OnReceiveMessage;
    public event Func<Task>? OnReady;
    public string Name => GetType().Name;
    public bool IsRunning => _webSocket.State == WebSocketState.Open;

    public TwitchClient(TwitchOptions options)
    {
        _options = options;
    }

    private async Task<bool> ValidateTokenAsync(string? token)
    {
        TwitchOAuth oauth = new(_options.ClientId);
        return token is not null && await oauth.ValidateTokenAsync(token);
    }

    public async Task<string> AuthAsync()
    {
        if (await ValidateTokenAsync(_options.Token))
        {
            return _options.Token!;
        }

        TwitchOAuth oauth = new(_options.ClientId);
        var response = await oauth.GetTokenAsync();
        var token = response?.AccessToken;
        if (token is null)
        {
            throw new NullReferenceException("Twitch Token");
        }
        _options.Token = token;
        return _options.Token;
    }

    public async Task RunAsync()
    {
        if (!await ValidateTokenAsync(_options.Token))
        {
            throw new InvalidOperationException("Failed to validate the token.");
        }

        await _webSocket.ConnectAsync(new Uri(_webSocketUrl), _cancellationTokenSource.Token);
        await SendMessageAsync($"PASS oauth:{_options.Token}");
        await SendMessageAsync($"NICK {_options.Username}");

        string? chan = _options.Channels.FirstOrDefault();
        if (!string.IsNullOrEmpty(chan))
        {
            await SendMessageAsync($"JOIN #{chan}");
        }

        if (OnReady is not null)
        {
            await OnReady.Invoke();
        }

        await ReceiveMessagesAsync(_cancellationTokenSource.Token);
    }


    public Task StopAsync()
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    public async Task SendMessageAsync(Message message) => await SendMessageAsync(message.Content);

    private async Task SendMessageAsync(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message + "\r\n");
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        var buffer = new byte[1024];
        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

            if (receivedMessage.Contains("PRIVMSG"))
            {
                // 例: :username!username@username.tmi.twitch.tv PRIVMSG #channelname :This is a message
                string[] parts = receivedMessage.Split(new[] { "!", "@", "PRIVMSG" }, StringSplitOptions.RemoveEmptyEntries);
                string username = parts[0].TrimStart(':');
                string channel = parts[3].Substring(0, parts[3].IndexOf(':', 1)).Trim();
                string message = parts[3].Substring(parts[3].IndexOf(':', 1) + 1).Trim();

                if (OnReceiveMessage is not null)
                {
                    await OnReceiveMessage.Invoke(new(Name, channel, username, message, DateTime.Now, null));
                }
            }
            else if (receivedMessage.Contains("PING"))
            {
                // PING応答
                await SendMessageAsync("PONG :tmi.twitch.tv");
            }
        }
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}