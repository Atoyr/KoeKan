using Medoz.KoeKan.Clients;
using Medoz.CatChast.Messaging;

using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Services;

/// <summary>
/// スピーカーの管理を行うクラス
/// </summary>
public class SpeakerService : ISpeakerService
{
    private readonly Dictionary<string, ISpeakerClient> _clients = new();
    private readonly Dictionary<string, IDisposable> _subscriptions = new();

    private readonly string _defaultClient = "_";

    private readonly IAsyncEventBus _asyncEventBus;
    private readonly ILogger _logger;

    /// <summary>
    /// スピーカーの管理を行うクラス
    /// </summary>
    /// <param name="listenerService"></param>
    /// <param name="configService"></param>
    public SpeakerService(
        IAsyncEventBus asyncEventBus,
        ILogger<SpeakerService> logger)
    {
        _asyncEventBus = asyncEventBus;
        _logger = logger;
    }

    /// <summary>
    /// クライアントの取得
    /// </summary>
    public ISpeakerClient GetSpeaker(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            if (_clients.TryGetValue(_defaultClient, out var defaultClient))
            {
                return defaultClient;
            }
            throw new KeyNotFoundException($"Default speaker client '{_defaultClient}' not found.");
        }

        if (_clients.TryGetValue(name, out var client))
        {
            return client;
        }
        throw new KeyNotFoundException($"Speaker client '{name}' not found.");
    }

    public bool TryGetSpeaker(string? name, out ISpeakerClient? client)
    {
        var clientName = string.IsNullOrEmpty(name) ? _defaultClient : name;
        return _clients.TryGetValue(clientName, out client);
    }

    /// <summary>
    /// スピーカー取得または作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetOrCreateSpeaker<T>(
        IClientOptions options,
        string name
        ) where T : ISpeakerClient
    {
        if (_clients.ContainsKey(name))
        {
            if (_clients[name] is T registeredClient)
            {
                return registeredClient;
            }
            throw new InvalidOperationException($"Client {name} is already registered with a different type.");
        }

        return CreateSpeaker<T>(options, name);
    }

    /// <summary>
    /// スピーカー作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public T CreateSpeaker<T>(
        IClientOptions options,
        string name
        ) where T : ISpeakerClient
    {
        if (_clients.ContainsKey(name))
        {
            throw new ArgumentException($"Client {name} is already registered.");
        }

        var speaker = ClientFactory.Create<T>(options);
        _clients.Add(name, speaker);

        var subscription = _asyncEventBus.SubscribeAsync<ClientMessage>(async e =>
        {
            if (e.Key == name)
            {
                await speaker.SpeakMessageAsync(e.Content);
            }
        });
        _subscriptions.Add(name, subscription);

        speaker.OnReady += async () =>
        {
            _logger.LogInformation($"{name} client started successfully.");
            await Task.CompletedTask;
        };
        return speaker;
    }

    /// <summary>
    /// クライアントの登録
    /// </summary>
    public void RegisterSpeaker(string name, ISpeakerClient client)
    {
        if (_clients.ContainsKey(name))
        {
            throw new ArgumentException($"Client {name} is already registered.");
        }
        _clients.Add(name, client);

        var subscription = _asyncEventBus.SubscribeAsync<ClientMessage>(async e =>
        {
            if (e.Key == name)
            {
                await client!.SpeakMessageAsync(e.Content);
            }
        });
        _subscriptions.Add(name, subscription);
    }

    /// <summary>
    /// クライアントの削除
    /// </summary>
    public void RemoveSpeaker(string name)
    {
        if (_clients.ContainsKey(name))
        {
            _clients.Remove(name);
            if (_subscriptions.TryGetValue(name, out var subscription))
            {
                subscription.Dispose();
                _subscriptions.Remove(name);
            }
        }
        else
        {
            throw new ArgumentException($"Client {name} is not registered.");
        }
    }
}
