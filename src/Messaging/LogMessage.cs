using Microsoft.Extensions.Logging;

namespace Medoz.CatChast.Messaging;

public record LogMessage(
    string ClientType,
    string SourceName,
    string Content,
    LogLevel LogLevel = LogLevel.Information,
    string? SpeakerName = null)
{
    public LogMessage(string clientType, string content)
        : this(clientType, "system", content)
    {

    }

    /// <summary>
    /// コンテンツだけのメッセージを作成します
    /// </summary>
    /// <param name="content"></param>
    public LogMessage(string content)
        : this("log", "system", content)
    {
    }

    /// <summary>
    /// コンテンツだけのメッセージを作成します
    /// </summary>
    /// <param name="content"></param>
    public LogMessage(string content, LogLevel logLevel)
        : this("log", "system", content, logLevel)
    {
    }
}

