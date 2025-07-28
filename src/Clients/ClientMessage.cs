namespace Medoz.KoeKan.Clients;

public record ClientMessage(
    string Key,
    string ClientType,
    string Channel,
    string Username,
    string Content,
    DateTimeOffset Timestamp,
    string? IconSource = null)
    {
        public ClientMessage(string key, string clientType, string channel, string username, string content, string? iconSource = null)
            : this(key, clientType, channel, username, content, DateTimeOffset.UtcNow, iconSource)
            {

            }

    }