using Medoz.MessageTransporter.Clients;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public record TwitchConfig(IEnumerable<string> Channels, bool UseSpeaker, uint? Speaker)
{
    public TwitchOptions ToTwitchOptions()
        => new TwitchOptions(Channels);
}