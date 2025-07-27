using Medoz.CatChast.Messaging;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

using Microsoft.Extensions.Logging;

using Message = Medoz.CatChast.Messaging.Message;

namespace Medoz.KoeKan.Services;

/// <summary>
/// クライアントの管理を行うクラス
/// </summary>
public class ClientService : IClientService
{
    private readonly Dictionary<string, ITextClient> _clients = new();

    private readonly string _defaultClient = "_";

    private readonly IConfigService _configService;
    private readonly IAsyncEventBus _asyncEventBus;
    private readonly ILogger _logger;

    /// <summary>
    /// クライアントの管理を行うクラス
    /// </summary>
    /// <param name="asyncEventBus"></param>
    /// <param name="configService"></param>
    public ClientService(
        IAsyncEventBus asyncEventBus,
        IConfigService configService,
        ILogger<ClientService> logger)
    {
        _configService = configService;
        _asyncEventBus = asyncEventBus;
        _logger = logger;
        AddDefaultClient();
    }

    /// <summary>
    /// デフォルトクライアントを追加します。
    /// </summary>
    private void AddDefaultClient()
    {
        var client = ClientFactory.Create<EchoClient>(new EchoOptions());
        _clients.Add(_defaultClient, client);

        client.OnReceiveMessage += async (message) =>
        {
            await _asyncEventBus.PublishAsync(new Message(
                // FIXME: Type is not used in this context, consider removing it
                "echo",
                message.Channel,
                message.Username,
                message.Content,
                message.Timestamp,
                message.IconSource
            ));
        };
        client.RunAsync().Wait();
    }

    /// <summary>
    /// クライアントの取得
    /// </summary>
    public ITextClient GetClient(string? name)
    {
        if (_clients.TryGetValue(name ?? _defaultClient, out var client))
        {
            return client;
        }
        throw new ArgumentException($"Client {name} is not registered.");
    }

    private ITextClient GetClientOrDefault(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return _clients[_defaultClient];
        }
        return _clients.TryGetValue(name, out var client) ? client : _clients[_defaultClient];
    }

    public bool TryGetClient(string? name, out ITextClient? client)
    {
        var clientName = string.IsNullOrEmpty(name) ? _defaultClient : name;
        return _clients.TryGetValue(clientName, out client);
    }

    public T GetOrCreateClient<T>(
        IClientOptions options,
        string name,
        Func<ClientMessage, Task>? onReceiveMessage = null
        ) where T : ITextClient
    {
        if (_clients.TryGetValue(name, out var registeredClient))
        {
            if (registeredClient is T typedClient)
            {
                return typedClient;
            }
            else
            {
                throw new ArgumentException($"Client {name} is already registered with a different type.");
            }
        }

        return CreateClient<T>(options, name, onReceiveMessage);
    }

    public T CreateClient<T>(
        IClientOptions options,
        string name,
        Func<ClientMessage, Task>? onReceiveMessage = null
        ) where T : ITextClient
    {
        if (_clients.ContainsKey(name))
        {
            throw new ArgumentException($"Client {name} is already registered.");
        }

        var client = ClientFactory.Create<T>(options);
        _clients.Add(name, client);

        if (onReceiveMessage is null)
        {
            client.OnReceiveMessage += async (message) =>
            {
                await _asyncEventBus.PublishAsync(new Message(
                    // FIXME: Type is not used in this context, consider removing it
                    nameof(T),
                    message.Channel,
                    message.Username,
                    message.Content,
                    message.Timestamp,
                    message.IconSource
                ));
            };
        }
        else
        {
            client.OnReceiveMessage += onReceiveMessage;
        }

        client.OnReady += async () =>
        {
            _logger.LogInformation($"{name} client started successfully.");
            await Task.CompletedTask;
        };

        return client;
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
        client.OnReceiveMessage += async message =>
        {
            await _asyncEventBus.PublishAsync(message);
        };
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
}
