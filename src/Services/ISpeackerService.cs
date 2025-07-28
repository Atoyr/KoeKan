using Medoz.KoeKan.Speakers;

namespace Medoz.KoeKan.Services;

public interface ISpeakerService
{
    /// <summary>
    /// クライアントの取得
    /// </summary>
    ISpeaker GetSpeaker(string? name = null);

    /// <summary>
    /// クライアントの取得
    /// </summary>
    bool TryGetSpeaker(string? name, out ISpeaker? client);

    /// <summary>
    /// スピーカー取得または作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    T GetOrCreateSpeaker<T>(
        ISpeakerOptions options,
        string name
        ) where T : ISpeaker;

    /// <summary>
    /// スピーカー作成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    T CreateSpeaker<T>(
        ISpeakerOptions options,
        string name
        ) where T : ISpeaker;

    /// <summary>
    /// クライアントの登録
    /// </summary>
    void RegisterSpeaker(string name, ISpeaker client);

    /// <summary>
    /// クライアントの削除
    /// </summary>
    void RemoveSpeaker(string name);
}
