namespace Medoz.MessageTransporter.Client;

public record Message(ClientType Source, string Channel, string Username, string Content)
{
}