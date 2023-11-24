namespace Medoz.TextTransporter.Client;

public record TwitchOptions(string Token, string Username, string Channel)
{
    public string Uri { get => "wss://irc-ws.chat.twitch.tv:443"; }
}