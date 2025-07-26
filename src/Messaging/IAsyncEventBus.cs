namespace Medoz.CatChast.Messaging;

/// <summary>
/// Provides an asynchronous event bus for publishing and subscribing to events.
/// </summary>
public interface IAsyncEventBus
{
    /// <summary>
    /// Subscribes a synchronous handler to events of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    IDisposable Subscribe<T>(Action<T> handler);

    /// <summary>
    /// Subscribes an asynchronous handler to events of type T.
    /// </summary>
    IDisposable SubscribeAsync<T>(Func<T, Task> handler);

    /// <summary>
    /// Unsubscribes a synchronous handler from events of type T.
    /// </summary>
    void Unsubscribe<T>(Action<T> handler);

    /// <summary>
    /// Unsubscribes an asynchronous handler from events of type T.
    /// </summary>
    void UnsubscribeAsync<T>(Func<T, Task> handler);

    /// <summary>
    /// Publishes an event to all subscribed handlers asynchronously.
    /// </summary>
    Task PublishAsync<T>(T eventData);

    /// <summary>
    /// Removes all subscribed handlers.
    /// </summary>
    void Clear();
}