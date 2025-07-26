namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public enum ChatMessageType
{
    Command,
    LogSuccess,
    LogInfo,
    LogWarning,
    LogFatal,
    Echo,
    Discord,
    Twitch,
}

public static class ChatMessageTypeExtension
{
    public static string ToString(this ChatMessageType type)
    {
        return type switch
        {
            ChatMessageType.Command => "Command",
            ChatMessageType.LogSuccess => "LogSuccess",
            ChatMessageType.LogInfo => "LogInfo",
            ChatMessageType.LogWarning => "LogWarning",
            ChatMessageType.LogFatal => "LogFatal",
            ChatMessageType.Echo => "Echo",
            ChatMessageType.Discord => "Discord",
            ChatMessageType.Twitch => "Twitch",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static ChatMessageType FromString(string type)
    {
        return type.ToUpper() switch
        {
            "COMMAND" => ChatMessageType.Command,
            "LOGSUCCESS" => ChatMessageType.LogSuccess,
            "LOGINFO" => ChatMessageType.LogInfo,
            "LOGWARNING" => ChatMessageType.LogWarning,
            "LOGFATAL" => ChatMessageType.LogFatal,
            "ECHO" => ChatMessageType.Echo,
            "DISCORD" => ChatMessageType.Discord,
            "TWITCH" => ChatMessageType.Twitch,
            _ => throw new ArgumentException($"Unknown ChatMessageType: {type}", nameof(type))
        };
    }
}