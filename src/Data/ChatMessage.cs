namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public record ChatMessage(
        ChatMessageType MessageType,
        string Channel,
        string? IconSource,
        string Username,
        string Message,
        DateTime Timestamp,
        bool IsConsecutiveMessage)
{
}