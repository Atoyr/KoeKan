namespace Medoz.MessageTransporter.Clients;

public record Message(ClientType ClientType, string Channel, string Username, string Content, DateTime Timestamp) { }