namespace Medoz.MessageTransporter.Clients;

public record Message(ClientType Source, string Channel, string Username, string Content)
{
}