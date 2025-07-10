using Medoz.KoeKan.Clients;

namespace Medoz.KoeKan.Services;

public interface IClientService
{
    /// <summary>
    /// クライアントの取得
    /// </summary>
    ITextClient GetClient(string? name = null);

    /// <summary>
    /// クライアントの登録
    /// </summary>
    void RegisterClient(string name, ITextClient client);

    /// <summary>
    /// クライアントの削除
    /// </summary>
    void RemoveClient(string name);

    void AppendSpeaker(ISpeakerClient speaker, string? clientName = null);

    void RemoveSpeaker(string? clientName = null);
}