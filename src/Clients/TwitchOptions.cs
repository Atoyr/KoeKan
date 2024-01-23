namespace Medoz.MessageTransporter.Clients;

public record TwitchOptions(IEnumerable<string> Channels)
{
    public string Uri { get => "wss://irc-ws.chat.twitch.tv:443"; }

    // Client TypeがPublicなので、ClientSecretは不要
    public readonly string ClientId = "rgl0q1gsjromlw3ro7z8n4p2g9w34u";

    public readonly string Username = "MessageTransporter";
}