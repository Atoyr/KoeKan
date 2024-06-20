using Medoz.KoeKan.Clients;

namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public record DiscordConfig(ulong? DefaultChannelId, bool UseSpeaker, uint? Speaker)
{
}