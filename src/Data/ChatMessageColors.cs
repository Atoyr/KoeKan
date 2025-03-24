
using System.Drawing;

namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public class ChatMessageColors
{
    private readonly Dictionary<ChatMessageType, string> _options = new();

    private readonly string _defaultColor = "#000000";

    public string this[ChatMessageType key]
    {
        get
        {
            if (!_options.ContainsKey(key))
            {
                return _defaultColor;
            }
            return _options[key];
        }
        set => _options[key] = value;
    }

    public static ChatMessageColors Default()
    {
        ChatMessageColors colors = new();
        colors[ChatMessageType.Echo] = "#ff7b2e";
        colors[ChatMessageType.LogSuccess] = "#FFADFF2F";
        colors[ChatMessageType.LogInfo] = "#FF1E90FF";
        colors[ChatMessageType.LogWarning] = "#FFDAA520";
        colors[ChatMessageType.LogFatal] = "#FFFF0000";
        colors[ChatMessageType.Discord] = "#5865F2";
        colors[ChatMessageType.Twitch] = "#9147ff";
        return colors;
    }
}