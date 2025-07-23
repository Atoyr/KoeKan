using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Navigation;

namespace Medoz.CatChast.Messaging;

public class AsyncEventBus : IAsyncEventBus
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, Delegate>> _handlers = new();

    private bool TryAdd(Type type, Delegate handler)
    {
        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = new ConcurrentDictionary<int, Delegate>();
        }
        return _handlers[type].TryAdd(handler.GetHashCode(), handler);
    }

    private bool TryRemove(Type type, Delegate handler)
    {
        if (_handlers.ContainsKey(type))
        {
            return _handlers[type].TryRemove(handler.GetHashCode(), out _);
        }
        return false;
    }

    public IDisposable Subscribe<T>(Action<T> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        TryAdd(typeof(T), handler);
        return new SubscriptionToken(() => TryRemove(typeof(T), handler));
    }

    public IDisposable SubscribeAsync<T>(Func<T, Task> asyncHandler)
    {
        if (asyncHandler is null)
            throw new ArgumentNullException(nameof(asyncHandler));

        TryAdd(typeof(T), asyncHandler);
        return new SubscriptionToken(() => TryRemove(typeof(T), asyncHandler));
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        TryRemove(typeof(T), handler);
    }

    public void UnsubscribeAsync<T>(Func<T, Task> asyncHandler)
    {
        if (asyncHandler is null)
            throw new ArgumentNullException(nameof(asyncHandler));

        TryRemove(typeof(T), asyncHandler);
    }

    public async Task PublishAsync<T>(T eventData)
    {
        if (eventData is null)
            return;

        List<Delegate> handlers;
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
            return;

        handlers = _handlers[eventType].Select(kv => kv.Value).ToList();

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Action<T> syncHandler)
                {
                    // 同期ハンドラーをTaskで包む
                    tasks.Add(Task.Factory.StartNew(() => syncHandler(eventData)));

                }
                else if (handler is Func<T, Task> asyncHandler)
                {
                    // 非同期ハンドラー
                    tasks.Add(asyncHandler(eventData));
                }
            }
            catch (Exception ex)
            {
                // TODO: Replace with proper logging mechanism
                System.Diagnostics.Debug.WriteLine($"Error creating task for event handler: {ex.Message}");
            }
        }

        // すべてのハンドラーの完了を待つ
        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in event handlers: {ex.Message}");
            }
        }
    }

    public void Clear()
    {
        _handlers.Clear();
    }
}
