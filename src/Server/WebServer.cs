// WebServer.cs
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medoz.KoeKan.Server;
internal class WebServer
{
    private HttpListener? listener;
    private readonly Action<string, string> _messageCallback;
    private readonly Action _clientConnectedCallback;
    private readonly Action _clientDisconnectedCallback;
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();
    private bool isRunning = false;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public WebServer(
        Action<string, string> messageCallback,
        Action clientConnectedCallback,
        Action clientDisconnectedCallback)
    {
        _messageCallback = messageCallback;
        _clientConnectedCallback = clientConnectedCallback;
        _clientDisconnectedCallback = clientDisconnectedCallback;
    }

    public void Start(int port)
    {
        if (isRunning)
            return;

        listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{port}/");
        listener.Start();

        isRunning = true;

        Task.Run(() => ListenForClientsAsync(_cancellationTokenSource.Token));
    }

    public void Stop()
    {
        if (!isRunning)
            return;

        isRunning = false;
        _cancellationTokenSource.Cancel();

        // 全クライアントを切断
        foreach (var client in _clients)
        {
            try
            {
                client.Value.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).Wait();
            }
            catch { }
        }

        _clients.Clear();

        listener?.Stop();
        listener?.Close();
    }

    public int GetClientCount()
    {
        return _clients.Count;
    }

    public async void BroadcastMessage(string sender, string message)
    {
        if (!isRunning)
            return;

        var messageObj = new
        {
            type = "message",
            sender = sender,
            content = message,
            timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        string jsonMessage = System.Text.Json.JsonSerializer.Serialize(messageObj);
        byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage);

        List<string> disconnectedClients = new List<string>();

        foreach (var client in _clients)
        {
            try
            {
                await client.Value.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                disconnectedClients.Add(client.Key);
            }
        }

        // 切断されたクライアントを削除
        foreach (var clientId in disconnectedClients)
        {
            RemoveClient(clientId);
        }
    }

    private async Task ListenForClientsAsync(CancellationToken cancellationToken)
    {
        // FIXME: throw error
        if (listener == null)
            return;
        while (!cancellationToken.IsCancellationRequested && isRunning)
        {
            try
            {
                var context = await listener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    await ProcessWebSocketRequest(context, cancellationToken);
                }
                else
                {
                    // 通常のHTTPリクエストを処理
                    await ProcessHttpRequest(context);
                }
            }
            catch (Exception)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }

    private async Task ProcessWebSocketRequest(HttpListenerContext context, CancellationToken cancellationToken)
    {
        try
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;

            string clientId = Guid.NewGuid().ToString();
            _clients.TryAdd(clientId, webSocket);
            _clientConnectedCallback?.Invoke();

            await HandleWebSocketClient(clientId, webSocket, cancellationToken);
        }
        catch (Exception)
        {
            context.Response.StatusCode = 500;
            context.Response.Close();
        }
    }

    private async Task HandleWebSocketClient(string clientId, WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        try
        {
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    RemoveClient(clientId);
                    break;
                }

                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                try
                {
                    var message = System.Text.Json.JsonSerializer.Deserialize<Message>(jsonMessage);
                    if (message != null && message.Type == "message")
                    {
                        // メッセージを他のクライアントに送信
                        _messageCallback?.Invoke(message.Sender, message.Content);
                        BroadcastMessage(message.Sender, message.Content);
                    }
                }
                catch { }
            }
        }
        catch
        {
            // 例外が発生した場合、クライアントを削除
            RemoveClient(clientId);
        }
    }

    private void RemoveClient(string clientId)
    {
        if (_clients.TryRemove(clientId, out _))
        {
            _clientDisconnectedCallback?.Invoke();
        }
    }

    private async Task ProcessHttpRequest(HttpListenerContext context)
    {
        if (context.Request.Url is null)
            return;
        string path = context.Request.Url.AbsolutePath;

        if (path == "/" || string.IsNullOrEmpty(path))
        {
            try
            {
                // HTMLはリソースファイルまたは別のクラスから取得
                string htmlContent = HtmlContent.ChatClientHtml;
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    context.Response.ContentType = "text/html";
                    await writer.WriteAsync(htmlContent);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    await writer.WriteAsync($"Error: {ex.Message}");
                }
            }
        }
        else
        {
            context.Response.StatusCode = 404;
        }

        context.Response.Close();
    }
}
