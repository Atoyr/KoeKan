using System.Collections.ObjectModel;
using System.Windows.Threading;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Clients;

public class EchoClient: ITextClient
{
    private bool isRunning = false;

    private readonly IClientOptions _options;

    public EchoClient(IClientOptions options)
    {
        _options = options;
    }

    public event Func<Task>? OnReady;

    public event Func<Message, Task>? OnReceiveMessage;

    public Task<string> AuthAsync()
    {
        return Task.FromResult("EchoClient");
    }

    public async Task RunAsync()
    {
        isRunning = true;
        if (OnReady is not null)
        {
            await OnReady.Invoke();
        }
    }

    public Task StopAsync()
    {
        isRunning = false;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public async Task SendMessageAsync(Message message)
    {
        if (!isRunning)
        {
            return;
        }

        if (OnReceiveMessage is not null)
        {
            await OnReceiveMessage.Invoke(message);
        }
    }
}