namespace Medoz.KoeKan.Clients;

public record Message(ClientType ClientType, string Channel, string Username, string Content, DateTime Timestamp, string? IconSource) { }