using Medoz.CatChast.Messaging;
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

using Medoz.CatChast.Messaging;

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

    /// <summary>
    /// クライアントの管理を行うクラス
    /// </summary>
    /// <param name="asyncEventBus"></param>
    /// <param name="configService"></param>
    public ClientService(IAsyncEventBus asyncEventBus, IConfigService configService)
    {
        _configService = configService;
        _asyncEventBus = asyncEventBus;
        AddDefaultClient();
    }

    /// <summary>
    /// デフォルトクライアントを追加します。
    /// </summary>
    private void AddDefaultClient()
    {
        var client = new EchoClient(new EchoOptions());
        client.OnReceiveMessage += async message =>
        {
            await _asyncEventBus.PublishAsync(message);
        };
        client.RunAsync().Wait();
        _clients.Add(_defaultClient, client);

        var config = _configService.GetConfig();
        // listener?.AddMessageConverter(
        //     _defaultClient,
        //     (message) => new ChatMessage(
        //         ChatMessageType.Echo,
        //         "",
        //         config.Icon,
        //         config.Username,
        //         message.Content,
        //         message.Timestamp,
        //         false));
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

    public bool TryGetClient(string? name, out ITextClient? client)
    {
        var clientName = string.IsNullOrEmpty(name) ? _defaultClient : name;
        return _clients.TryGetValue(clientName, out client);
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
