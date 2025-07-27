
using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Medoz.CatChast.Messaging;

public class EventBusLoggerProvider : ILoggerProvider
{
    private readonly EventBusLoggerConfiguration _config;
    private readonly ConcurrentDictionary<string, EventBusLogger> _loggers = new();

    private readonly IAsyncEventBus _asyncEventBus;

    public EventBusLoggerProvider(IAsyncEventBus asyncEventBus, EventBusLoggerConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _asyncEventBus = asyncEventBus ?? throw new ArgumentNullException(nameof(asyncEventBus));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new EventBusLogger(_asyncEventBus, _config));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}