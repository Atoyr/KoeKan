using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Server;

using Discord.Rest;

namespace Medoz.KoeKan.Services;

internal class ServerService : IServerService, IDisposable
{

    private readonly WebApi _webApi;

    public event EventHandler<string>? WebApiMessageReceived;


    public ServerService()
    {
        _webApi = new WebApi((request) =>
        {
            // Handle incoming request here
            WebApiMessageReceived?.Invoke(this, request.Message);
        });
    }

    public ServerService(WebApi webApi)
    {
        _webApi = webApi;
    }

    public async Task StartWebApiAsync()
    {
        await _webApi.StartAsync(22222);
    }
    public void StopWebApi()
    {
        _webApi.Stop();
    }

    public void Dispose()
    {
        _webApi.Dispose();
        // Dispose resources if any
    }
}
