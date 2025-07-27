namespace Medoz.CatChast.Messaging;

/// <summary>
/// サブスクリプショントークン
/// </summary>
public class SubscriptionToken : IDisposable
{
    private readonly Action _unsubscribeAction;
    private bool _disposed = false;

    public SubscriptionToken(Action unsubscribeAction)
    {
        _unsubscribeAction = unsubscribeAction ?? throw new ArgumentNullException(nameof(unsubscribeAction));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _unsubscribeAction();
            _disposed = true;
        }
    }
}