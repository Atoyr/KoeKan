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
    /// クライアントの登録
    /// </summary>
    void RegisterSpeaker(string name, ISpeakerClient client);

    /// <summary>
    /// クライアントの削除
    /// </summary>
    void RemoveSpeaker(string name);
}
