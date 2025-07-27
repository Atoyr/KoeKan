using Medoz.KoeKan.Clients;

namespace Medoz.KoeKan.Services;

public interface ISpeakerService
{
    /// <summary>
    /// クライアントの取得
    /// </summary>
    ISpeakerClient GetSpeaker(string? name = null);

    /// <summary>
    /// クライアントの取得
    /// </summary>
    bool TryGetSpeaker(string? name, out ISpeakerClient? client);

    /// <summary>
    /// スピーカー取得または作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    T GetOrCreateSpeaker<T>(
        IClientOptions options,
        string name
        ) where T : ISpeakerClient;

    /// <summary>
    /// スピーカー作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    T CreateSpeaker<T>(
        IClientOptions options,
        string name
        ) where T : ISpeakerClient;

    /// <summary>
    /// クライアントの登録
    /// </summary>
    void RegisterSpeaker(string name, ISpeakerClient client);

    /// <summary>
    /// クライアントの削除
    /// </summary>
    void RemoveSpeaker(string name);
}
