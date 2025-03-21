using Medoz.KoeKan.Clients;

namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public record TwitchConfig(IEnumerable<string> Channels, bool UseSpeaker, uint? Speaker)
{
    public TwitchOptions ToTwitchOptions()
        => new TwitchOptions()
        {
            Channels = Channels
        };
}