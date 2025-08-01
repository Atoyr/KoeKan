using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medoz.CatChast.Messaging;

// 拡張メソッド
public static class EventBusLoggerExtensions
{
    public static ILoggingBuilder AddEventBusLogger(this ILoggingBuilder builder, EventBusLoggerConfiguration config)
    {
        builder.Services.AddSingleton<ILoggerProvider>(serviceProvider =>
        {
            var asyncEventBus = serviceProvider.GetRequiredService<IAsyncEventBus>();
            return new EventBusLoggerProvider(asyncEventBus, config);
        });
        return builder;
    }
    public static ILoggingBuilder AddEventBusLogger(this ILoggingBuilder builder, EventBusLoggerConfiguration config, IAsyncEventBus asyncEventBus)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new EventBusLoggerProvider( asyncEventBus, config));
        return builder;
    }

    public static ILoggingBuilder AddEventBusLogger(this ILoggingBuilder builder, Action<EventBusLoggerConfiguration> configure)
    {
        var config = new EventBusLoggerConfiguration();
        configure(config);
        return builder.AddEventBusLogger(config);
    }
}