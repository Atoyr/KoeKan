namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public record ChatMessage(ChatMessageType MessageType, string Channel, string Username, string Message, DateTime Timestamp, string? IconSource)
{
}