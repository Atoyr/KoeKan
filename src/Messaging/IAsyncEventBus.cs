namespace Medoz.CatChast.Messaging;

public interface IAsyncEventBus
{
    void Subscribe<T>(Action<T> handler);
    void SubscribeAsync<T>(Func<T, Task> handler);
    void Unsubscribe<T>(Action<T> handler);
    void UnsubscribeAsync<T>(Func<T, Task> handler);
    Task PublishAsync<T>(T eventData);
    void Clear();
}