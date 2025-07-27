namespace Medoz.CatChast.Messaging;

public record Message(
    string ClientType,
    string Channel,
    string Username,
    string Content,
    DateTimeOffset Timestamp,
    string? IconSource = null,
    string? SpeakerName = null)
{
    public Message(string clientType, string channel, string username, string content, string? iconSource = null)
        : this(clientType, channel, username, content, DateTimeOffset.UtcNow, iconSource)
    {

    }

    /// <summary>
    /// コンテンツだけのメッセージを作成します
    /// </summary>
    /// <param name="content"></param>
    public Message(string content, string clientType = "")
        : this(clientType, "", "", content, DateTimeOffset.UtcNow)
    {
    }
}
