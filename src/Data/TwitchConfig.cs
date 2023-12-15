using Medoz.MessageTransporter.Clients;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public record TwitchConfig(string ClientId, string Secret, string Username, IEnumerable<string> Channels, bool UseSpeaker, uint? Speaker)
{
    public TwitchOptions ToTwitchOptions()
        => new TwitchOptions(ClientId, Secret, Username, Channels);
}