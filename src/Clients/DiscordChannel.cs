namespace Medoz.MessageTransporter.Clients;

public record DiscordChannel(ulong Id, string Name, ulong GuildId, string GuildName) { }