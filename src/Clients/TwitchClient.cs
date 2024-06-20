using System.Net.WebSockets;
using System.Text;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Clients;

public class TwitchClient: ITextClient
{
    private const string _oauthUri = "https://id.twitch.tv/oauth2/token";
    private ClientWebSocket _webSocket = new ClientWebSocket();

    private TwitchOptions _options;
    public TwitchOptions Options { get => _options; }

    private CancellationTokenSource _cancellationTokenSource = new();
    private string? _token;

    public event Func<Message, Task>? OnReceiveMessage;
    public event Func<Task>? OnReady;

    private ILogger? _logger;

    public TwitchClient(TwitchOptions options, ILogger? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    private async Task SetTokenAsync()
    {
        var s = Secret.Load();
        _token = s.DectyptTwitch();
        TwitchOAuth oauth = new(_options.ClientId);
        if (_token is not null && await oauth.ValidateTokenAsync(_token))
        {
            return;
        }
        var response = await oauth.GetTokenAsync();
        _token = response?.AccessToken;
        if (_token is null)
        {
            throw new NullReferenceException("Twitch Token");
        }
        else
        {
            s.EncryptTwitch(_token!);
            s.Save();
        }
    }

    public async Task RunAsync()
    {
        await SetTokenAsync();
        await _webSocket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), _cancellationTokenSource.Token);

        await SendMessageAsync($"PASS oauth:{_token}");
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

    public async Task SendMessageAsync(string message)
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
                _logger?.LogInformation("Twitch client is canceled");
                return;
            }

            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger?.LogInformation("Twitch client is canceled");

            if (receivedMessage.Contains("PRIVMSG"))
            {
                // 例: :username!username@username.tmi.twitch.tv PRIVMSG #channelname :This is a message
                string[] parts = receivedMessage.Split(new[] { "!", "@", "PRIVMSG" }, StringSplitOptions.RemoveEmptyEntries);
                string username = parts[0].TrimStart(':'); 
                string channel = parts[3].Substring(0, parts[3].IndexOf(':', 1)).Trim(); 
                string message = parts[3].Substring(parts[3].IndexOf(':', 1) + 1).Trim(); 

                if (OnReceiveMessage is not null)
                {
                    await OnReceiveMessage.Invoke(new(ClientType.Twitch, channel, username, message, DateTime.Now, null));
                }
            }
            else if (receivedMessage.Contains("PING"))
            {
                // PING応答
                await SendMessageAsync("PONG :tmi.twitch.tv");
                _logger?.LogInformation("Twitch: PING PONG ");
            }
        }
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}