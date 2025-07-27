using Microsoft.Extensions.Logging;

namespace  Medoz.CatChast.Messaging;
public class EventBusLoggerConfiguration
{
    public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
    public string OutputPath { get; set; } = string.Empty;
    public bool EnableConsoleOutput { get; set; } = true;
}