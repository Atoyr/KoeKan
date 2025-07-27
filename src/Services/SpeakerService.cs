using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.CatChast.Messaging;

using Message = Medoz.CatChast.Messaging.Message;

namespace Medoz.KoeKan.Services;

/// <summary>
/// スピーカーの管理を行うクラス
/// </summary>
public class SpeakerService : ISpeakerService
{
    private readonly Dictionary<string, ISpeakerClient> _clients = new();

    private readonly string _defaultClient = "_";

    private readonly IAsyncEventBus _asyncEventBus;

    /// <summary>
    /// スピーカーの管理を行うクラス
    /// </summary>
    /// <param name="listenerService"></param>
    /// <param name="configService"></param>
    public SpeakerService(IAsyncEventBus asyncEventBus)
    {
        _asyncEventBus = asyncEventBus;
    }

    /// <summary>
    /// クライアントの取得
    /// </summary>
    public ISpeakerClient GetSpeaker(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return _clients[_defaultClient];
        }
        return _clients[name];
    }

    public bool TryGetSpeaker(string? name, out ISpeakerClient? client)
    {
        var clientName = string.IsNullOrEmpty(name) ? _defaultClient : name;
        return _clients.TryGetValue(clientName, out client);
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

        _asyncEventBus.SubscribeAsync<Message>(async e =>
        {
            if (e.SpeakerName == name)
            {
                await client!.SpeakMessageAsync(e.Content);
            }
        });
    }

    /// <summary>
    /// クライアントの削除
    /// </summary>
    public void RemoveSpeaker(string name)
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
