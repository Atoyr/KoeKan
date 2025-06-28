using System.Reactive.Disposables;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public interface IServerService
{
    Task StartWebApiAsync();
    void StopWebApi();

    event EventHandler<string>? WebApiMessageReceived;
}

