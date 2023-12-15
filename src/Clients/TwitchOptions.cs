namespace Medoz.MessageTransporter.Clients;

public record TwitchOptions(string ClientId, string Secret, string Username, IEnumerable<string> Channels)
{
    public string Uri { get => "wss://irc-ws.chat.twitch.tv:443"; }
}