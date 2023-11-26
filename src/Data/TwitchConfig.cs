using Medoz.MessageTransporter.Clients;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public record TwitchConfig(string Token, string Username, string Channel)
{
    public TwitchOptions ToTwitchOptions()
        => new TwitchOptions(Token, Username, Channel);
}