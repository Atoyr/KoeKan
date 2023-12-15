namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public record ChatMessage(
        ChatMessageType MessageType, 
        string Channel, 
        string? IconSource, 
        string Username, 
        string Message, 
        DateTime Timestamp, 
        bool IsMessageOnly)
{
}