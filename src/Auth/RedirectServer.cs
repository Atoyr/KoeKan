using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Web;

namespace Medoz.CatChast.Auth;

/// <summary>
/// </summary>
public class RedirectServer : IDisposable
{
    public string[] RedirectUrls { get; }

    public event Action<HttpListenerContext>? OnRedirectReceived;

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
            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                continue;
            }

            if (redirectUrl.EndsWith("/"))
            {
                _listener.Prefixes.Add(redirectUrl);
            }
            else
            {
                _listener.Prefixes.Add(redirectUrl + "/");
            }

        }
        _listener.Start();
    }

    public async Task<HttpListenerContext> GetContextAsync()
    {
        if (_listener is null)
        {
            throw new InvalidOperationException("Redirect server is not started.");
        }

        if (!_listener.IsListening)
        {
            throw new InvalidOperationException("Redirect server is not listening.");
        }
        var context = await _listener.GetContextAsync();
        if (context is null)
        {
            throw new InvalidOperationException("Failed to get context from redirect server.");
        }
        OnRedirectReceived?.Invoke(context);
        return context;
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