using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace Medoz.CatChast.Auth;

/// <summary>
/// </summary>
public class RedirectServer : IDisposable
{
    public string[] RedirectUrls { get; }

    public RedirectServer(params string[] redirectUrls)
    {
        RedirectUrls = redirectUrls ?? throw new ArgumentNullException(nameof(redirectUrls));
        OpenRedirectServer();
    }

    private HttpListener? _listener;

    private void OpenRedirectServer()
    {
        if (_listener is not null && _listener.IsListening)
        {
            return;
        }

        if (_listener is not null)
        {
            CloseRedirectServer();
        }

        _listener = new();
        foreach (var redirectUrl in RedirectUrls)
        {
            _listener.Prefixes.Add(redirectUrl);
        }
        _listener.Start();
    }

    public async Task<HttpListenerContext> GetContextAsync()
    {
        if (_listener is null)
        {
            throw new InvalidOperationException("Redirect server is not started.");
        }

        return await _listener.GetContextAsync();
    }

    private void CloseRedirectServer()
    {
        if (_listener is not null)
        {
            _listener.Stop();
            _listener.Close();
            _listener = null;
        }
    }

    public void Dispose()
    {
        CloseRedirectServer();
    }
}