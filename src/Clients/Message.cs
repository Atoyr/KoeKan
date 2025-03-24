namespace Medoz.KoeKan.Clients;

public record Message(
    string ClientType,
    string Channel,
    string Username,
    string Content,
    DateTime Timestamp,
    string? IconSource = null)
    {
        public Message(string clientType, string channel, string username, string content, string? iconSource = null)
            : this(clientType, channel, username, content, DateTime.Now, iconSource)
            {

            }

    }