using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace Medoz.MessageTransporter.Clients;

/// <summary>
/// </summary>
internal class TwitchOAuth : IDisposable
{
    private const string _oauthTokenUri = "https://id.twitch.tv/oauth2/token";
    private const string _oauthAuthorizeUri = "https://id.twitch.tv/oauth2/authorize";
    private const string _space = "%20";

    public event Action<string>? OnTokenReceived;

    private HttpListener? _listener;
    private string _clientId;
    private int _redirectPort;

    public TwitchOAuth(string clientId, int redirectPort = 53919)
    {
        _clientId = clientId;
        _redirectPort = redirectPort;
    }

    public async Task<TwitchOAuthResponse?> GetTokenAsync()
    {
        string redirectUrl = $"http://localhost:{_redirectPort}";
        string url = $"{_oauthAuthorizeUri}?client_id={_clientId}&redirect_uri={redirectUrl}&response_type=token&scope=chat:read{_space}chat:edit";

        StartRedirectServer();
        Task<TwitchOAuthResponse?> redirect = WaitForRequest();

        ProcessStartInfo pi = new ProcessStartInfo()
        {
            FileName = url,
            UseShellExecute = true,
        };
        Process.Start(pi);

        return await redirect;
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");

        var response = await client.GetAsync("https://id.twitch.tv/oauth2/validate");
        return response.IsSuccessStatusCode;
    }

    public void Dispose()
    {
        CloseRedirectServer();
    }

    private void StartRedirectServer()
    {
        _listener = new();
        _listener.Prefixes.Add($"http://localhost:{_redirectPort}/");
        _listener.Start();
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

    private async Task<TwitchOAuthResponse?> WaitForRequest()
    {
        if (_listener is null)
        {
            // TODO throw Exception;
            return null;
        }

        HttpListenerContext? context = null;
        try
        {
            context = await _listener.GetContextAsync();
        }
        catch (Exception ex)
        {
            // エラー時の処理
        }

        if (context is not null)
        {
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/html";

            byte[] buffer = Encoding.UTF8.GetBytes(CreateResponseHtml());
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();

            var token = GetAccessToken(context);

            if (token is not null)
            {
                return token;
            }
        }
        return await WaitForRequest();
    }

    private string CreateResponseHtml()
    {
        return $@"
        <!DOCTYPE html><html><head></head><body></body>
          <script>
            var currentUrl = window.location.href;
            var fragment = window.location.hash;
            if (fragment) {{
              var newUrl = currentUrl.replace(fragment, '');
              fragment = fragment.replace('#', '?');
              if (newUrl.includes('?')) {{
                  fragment = fragment.replace('?', '&');
              }}
              window.location.replace(`http://localhost:{_redirectPort}/` + fragment);
            }}
          </script>
        </html>
    ";
    }

    private TwitchOAuthResponse? GetAccessToken(HttpListenerContext context)
    {
        TwitchOAuthResponse? response = null;
        if (context.Request.QueryString["access_token"] is not null)
        {
            uint expires_in = 0;
            uint.TryParse(context.Request.QueryString["expires_in"], out expires_in);
            response = new(
                context.Request.QueryString["access_token"]!,
                expires_in,
                context.Request.QueryString["scope"]?.Split(' ') ?? new string[0],
                context.Request.QueryString["token_type"] ?? string.Empty);
        }
        return response;
    }
}