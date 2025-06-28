using System.Collections.ObjectModel;
using System.Windows.Threading;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Clients;

public class EchoClient: ITextClient
{
    public bool IsRunning { get; private set; } = false;

    private readonly IClientOptions _options;

    public EchoClient(IClientOptions options)
    {
        _options = options;
    }

    public string Name => GetType().Name;

    public event Func<Task>? OnReady;

    public event Func<ClientMessage, Task>? OnReceiveMessage;

    public Task<string> AuthAsync()
    {
        return Task.FromResult("EchoClient");
    }

    public async Task RunAsync()
    {
        IsRunning = true;
        if (OnReady is not null)
        {
            await OnReady.Invoke();
        }
    }

    public Task StopAsync()
    {
        IsRunning = false;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public async Task SendMessageAsync(ClientMessage message)
    {
        if (!IsRunning)
        {
            return;
        }

        if (OnReceiveMessage is not null)
        {
            await OnReceiveMessage.Invoke(message);
        }
    }
}