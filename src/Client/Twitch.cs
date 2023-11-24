using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medoz.TextTransporter.Client;

public class Twitch
{
    private ClientWebSocket _webSocket = new ClientWebSocket();

    private TwitchOptions _options;

    public delegate void ReceiveMessageEventHandler(string message);

    public event ReceiveMessageEventHandler? OnReceiveMessage;

    public Twitch(TwitchOptions options)
    {
        _options = options;
    }

    public async Task ConnectAsync()
    {
        await _webSocket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), CancellationToken.None);
        await SendMessageAsync($"PASS oauth:{_options.Token}");
        await SendMessageAsync($"NICK {_options.Username}");
        await SendMessageAsync($"JOIN #{_options.Channel}");

        await ReceiveMessagesAsync();
    }

    public async Task SendMessageAsync(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message + "\r\n");
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024];
        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            OnReceiveMessage?.Invoke(message);
        }
    }
}