namespace Medoz.CatChast.Messaging;

public record Message(
    string ClientType,
    string Key,
    string Channel,
    string Username,
    string Content,
    DateTimeOffset Timestamp,
    string? IconSource = null)
{
    public Message(string clientType, string key,string channel, string username, string content, string? iconSource = null)
        : this(clientType, key, channel, username, content, DateTimeOffset.UtcNow, iconSource)
    {

    }
}
