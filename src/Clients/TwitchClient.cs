using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medoz.MessageTransporter.Clients;

public class TwitchClient: IClient
{
    private ClientWebSocket _webSocket = new ClientWebSocket();

    private TwitchOptions _options;
    private CancellationTokenSource _cancellationTokenSource = new();

    public event Func<Message, Task>? OnReceiveMessage;



    public TwitchClient(TwitchOptions options)
    {
        _options = options;
    }

    public async Task RunAsync()
    {
        await _webSocket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), CancellationToken.None);
        await SendMessageAsync($"PASS oauth:{_options.Token}");
        await SendMessageAsync($"NICK {_options.Username}");
        await SendMessageAsync($"JOIN #{_options.Channel}");

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
                return;
            }
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            if (OnReceiveMessage is not null)
            {
                await OnReceiveMessage.Invoke(new Message(ClientType.Twitch, _options.Channel, _options.Username, message));
            }
        }
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}