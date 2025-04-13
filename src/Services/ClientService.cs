using System.Runtime.CompilerServices;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public class ClientService : IClientService
{
    private readonly Dictionary<string, ITextClient> _clients = new();

    private readonly string _defaultClient = "_";

    private readonly IListenerService _listener;
    private readonly IConfigService _config;


    public ClientService(IListenerService listenerService, IConfigService configService)
    {
        _config = configService;
        _listener = listenerService;
        AddDefaultClient();
    }

    private void AddDefaultClient()
    {
        var client = new EchoClient(new EchoOptions());
        client.OnReceiveMessage += message => {
            _listener?.AddMessage(message);
            return Task.CompletedTask;
        };
        client.RunAsync().Wait();
        _clients.Add(_defaultClient, client);
        var listener = _listener.GetListener();
        var config = _config.GetConfig();
        listener?.AddMessageConverter(
            _defaultClient,
            (message) => new ChatMessage(
                ChatMessageType.Echo,
                "",
                config.Icon,
                config.Username,
                message.Content,
                message.Timestamp,
                false));
    }

    /// <summary>
    /// クライアントの取得
    /// </summary>
    public ITextClient GetClient(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return _clients[_defaultClient];
        }
        return _clients[name];
    }

    /// <summary>
    /// クライアントの登録
    /// </summary>
    public void RegisterClient(string name, ITextClient client)
    {
        if (_clients.ContainsKey(name))
        {
            throw new ArgumentException($"Client {name} is already registered.");
        }
        _clients.Add(name, client);
    }

    /// <summary>
    /// クライアントの削除
    /// </summary>
    public void RemoveClient(string name)
    {
        if (_clients.ContainsKey(name))
        {
            _clients.Remove(name);
        }
        else
        {
            throw new ArgumentException($"Client {name} is not registered.");
        }
    }

    public void AppendSpeaker(ISpeakerClient speaker, string? clientName = null)
    {
        if (clientName == null)
        {
            clientName = _defaultClient;
        }
        if (!_clients.ContainsKey(clientName))
        {
            throw new ArgumentException($"Client {clientName} is not registered.");
        }

        _clients[clientName] = new TextToSpeechBridge(_clients[clientName], speaker);
    }

    public void RemoveSpeaker(string? clientName = null)
    {
        if (clientName == null)
        {
            clientName = _defaultClient;
        }
        if (_clients.ContainsKey(clientName))
        {
            throw new ArgumentException($"Client {clientName} is not registered.");
        }

        if (_clients[clientName] is TextToSpeechBridge bridge)
        {
            _clients[clientName] = bridge.GetTextClient();
            bridge.Dispose();
        }
    }
}
