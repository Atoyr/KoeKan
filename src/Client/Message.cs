namespace Medoz.TextTransporter.Client;

public record Message(string Source, string Channel, string Username, string Content)
{
}
