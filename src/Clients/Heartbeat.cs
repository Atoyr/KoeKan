using System;
using System.Threading;
namespace Medoz.MessageTransporter.Clients;

public class Heartbeat : IDisposable
{
    private Timer _timer;

    public event Action<object?>? OnHeartbeat;

    public Heartbeat(TimeSpan interval)
    {
        // TimerCallback デリゲートを使用してタイマーのコールバックメソッドを設定
        _timer = new Timer(HandleHeartbeat, null, TimeSpan.Zero, interval);
    }

    private void HandleHeartbeat(object? state)
        => OnHeartbeat?.Invoke(state);

    public void ChangeInterval(TimeSpan interval)
        => _timer.Change(TimeSpan.Zero, interval);

    public void Stop()
        => _timer.Change(Timeout.Infinite, Timeout.Infinite);

    public void Dispose()
    {
        _timer.Dispose();
    }
}