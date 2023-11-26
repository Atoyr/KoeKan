using Medoz.MessageTransporter.Clients;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public record DiscordConfig(string Token, ulong? DefaultChannelId)
{
    public DiscordOptions ToDiscordOptions()
        => new DiscordOptions(Token);
}