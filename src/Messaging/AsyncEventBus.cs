using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Medoz.CatChast.Messaging;

public class AsyncEventBus : IAsyncEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    private readonly object _lock = new object();

    public void Subscribe<T>(Action<T> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        lock (_lock)
        {
            var eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }
            _handlers[eventType].Add(handler);
        }
    }

    public void SubscribeAsync<T>(Func<T, Task> asyncHandler)
    {
        if (asyncHandler is null)
            throw new ArgumentNullException(nameof(asyncHandler));

        lock (_lock)
        {
            var eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }
            _handlers[eventType].Add(asyncHandler);
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            var eventType = typeof(T);
            if (_handlers.ContainsKey(eventType))
            {
                _handlers[eventType].Remove(handler);
                if (_handlers[eventType].Count == 0)
                {
                    _handlers.TryRemove(eventType, out _);
                }
            }
        }
    }

    public void UnsubscribeAsync<T>(Func<T, Task> asyncHandler)
    {
        if (asyncHandler is null)
            throw new ArgumentNullException(nameof(asyncHandler));

        lock (_lock)
        {
            var eventType = typeof(T);
            if (_handlers.ContainsKey(eventType))
            {
                _handlers[eventType].Remove(asyncHandler);
                if (_handlers[eventType].Count == 0)
                {
                    _handlers.TryRemove(eventType, out _);
                }
            }
        }
    }

    public async Task PublishAsync<T>(T eventData)
    {
        if (eventData is null)
            return;

        List<Delegate> handlers;
        lock (_lock)
        {
            var eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
                return;

            handlers = new List<Delegate>(_handlers[eventType]);
        }

        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Action<T> syncHandler)
                {
                    // 同期ハンドラーをTaskで包む
                    tasks.Add(Task.Run(() => syncHandler(eventData)));
                }
                else if (handler is Func<T, Task> asyncHandler)
                {
                    // 非同期ハンドラー
                    tasks.Add(asyncHandler(eventData));
                }
            }
            catch (Exception ex)
            {
                // エラーハンドリング
                Console.WriteLine($"Error creating task for event handler: {ex.Message}");
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
        lock (_lock)
        {
            _handlers.Clear();
        }
    }
}
