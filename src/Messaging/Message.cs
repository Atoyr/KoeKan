namespace Medoz.CatChast.Messaging;

public record Message(
    string ClientType,
    string Channel,
    string Username,
    string Content,
    DateTimeOffset Timestamp,
    string? IconSource = null)
    {
        public Message(string clientType, string channel, string username, string content, string? iconSource = null)
            : this(clientType, channel, username, content,  DateTimeOffset.UtcNow.DateTime, iconSource)
            {

            }
    }
