using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Medoz.CatChast.Messaging;
// カスタムロガーの実装
public class EventBusLogger : ILogger
{
    private readonly IAsyncEventBus _asyncEventBus;

    private readonly EventBusLoggerConfiguration _config;

    public EventBusLogger(IAsyncEventBus asyncEventBus, EventBusLoggerConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _asyncEventBus = asyncEventBus;
    }

    public IDisposable? BeginScope<TState>(TState state)
    {
        // スコープは特に必要ないので、nullを返す
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _config.MinLogLevel;
    }

    public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception? exception,
                                Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {message}";

        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }

        _asyncEventBus.PublishAsync(new LogMessage(logEntry, logLevel)).Wait();
    }
}