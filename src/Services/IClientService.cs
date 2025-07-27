using Medoz.KoeKan.Clients;

namespace Medoz.KoeKan.Services;

public interface IClientService
{
    /// <summary>
    /// クライアントの取得
    /// </summary>
    ITextClient GetClient(string? name = null);

    /// <summary>
    /// クライアントの取得
    /// </summary>
    bool TryGetClient(string? name, out ITextClient? client);

    /// <summary>
    /// クライアントの取得または作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <param name="onReceiveMessage"></param>
    /// <returns></returns>
    T GetOrCreateClient<T>(
        IClientOptions options,
        string name,
        Func<ClientMessage, Task>? onReceiveMessage
        ) where T : ITextClient;

    /// <summary>
    /// クライアントの作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <param name="onReceiveMessage"></param>
    /// <returns></returns>
    T CreateClient<T>(
        IClientOptions options,
        string name,
        Func<ClientMessage, Task>? onReceiveMessage
        ) where T : ITextClient;

    /// <summary>
    /// クライアントの登録
    /// </summary>
    void RegisterClient(string name, ITextClient client);

    /// <summary>
    /// クライアントの削除
    /// </summary>
    void RemoveClient(string name);
}