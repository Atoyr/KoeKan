using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Printing.IndexedProperties;

namespace Medoz.CatChast.Auth;

/// <summary>
/// </summary>
public class TwitchOAuthWithImplicit : TwitchOAuthBase, IDisposable
{
    /// <summary>
    /// スペースのURLエンコード
    /// </summary>
    private const string _space = "%20";

    internal TwitchOAuthOptions Options { get; }

    /// <summary>
    /// Twitch OAuthのクライアントID
    /// </summary>
    private readonly string _clientId;

    /// <summary>
    /// リダイレクトサーバーのポート番号
    /// </summary>
    private readonly int _redirectPort;

    /// <summary>
    /// リダイレクトURL
    /// </summary>
    private string _redirectUrl => $"http://localhost:{_redirectPort}";

    /// <summary>
    /// Twitch OAuthのスコープ
    /// </summary>
    internal string Scope => $"chat:read{_space}chat:edit";

    internal RedirectServer? RedirectServer { get; private set; }

    public TwitchOAuthWithImplicit(TwitchOAuthOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _clientId = options.ClientId;
        _redirectPort = options.RedirectPort;
    }


    /// <summary>
    /// Twitch OAuthの認証を行います。
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="Exception"></exception>
    public override async Task<TwitchOAuthToken> AuthorizeAsync(string? refreshToken = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_clientId))
        {
            throw new ArgumentException("Client ID is not set.");
        }

        if (_redirectPort <= 0 || _redirectPort > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(_redirectPort), "Redirect port must be between 1 and 65535.");
        }

        try
        {
            return await GetAccessTokenAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to authorize with Twitch OAuth.", ex);
        }
    }

    /// <summary>
    /// リダイレクトサーバーを起動し、Twitchの認証ページにリダイレクトします。
    /// </summary>
    /// <param name="state"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<HttpListenerContext?> RequestTwichAutorize(string state, CancellationToken cancellationToken = default)
    {
        // response_type=token
        string url = $"{OAuthAuthorizeUri}?client_id={_clientId}&redirect_uri={_redirectUrl}&response_type=token&scope={Scope}&state={state}";
        RedirectServer = new RedirectServer(_redirectUrl);

        // リダイレクトサーバーを起動して、認証コードを取得する
        HttpListenerContext? context = null;
        try
        {

            // OAuth認証ページを開く
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true,
            };
            Process.Start(pi);

            context = await WaitForRequest(RedirectServer, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                // キャンセルされた場合はnullを返す
                return null;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get authorization code.", ex);
        }

        return context;
    }

    /// <summary>
    /// リダイレクトサーバーからのリクエストを待機し、認証コードを取得します。
    /// </summary>
    /// <param name="server"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<HttpListenerContext?> WaitForRequest(RedirectServer server, CancellationToken cancellationToken = default)
    {
        server.OnRedirectReceived += context =>
        {
            var response = context!.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/html";

            byte[] buffer = Encoding.UTF8.GetBytes(CreateResponseHtml());
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        };

        // リダイレクトサーバーを起動して、認証コードを取得する
        HttpListenerContext? context = null;
        string? state = null;
        try
        {
            while (string.IsNullOrEmpty(state))
            {
                context = await server.GetContextAsync();
                state = context.Request.QueryString["state"];
                if (cancellationToken.IsCancellationRequested)
                {
                    // キャンセルされた場合はnullを返す
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get authorization code.", ex);
        }
        return context;
    }

    /// <summary>
    /// Twitch OAuthのアクセストークンを取得します。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="Exception"></exception>
    private async Task<TwitchOAuthToken> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        string state = Guid.NewGuid().ToString("N");
        var context = await RequestTwichAutorize(state, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            // キャンセルされた場合はnullを返す
            throw new OperationCanceledException("Authorization was canceled by the user.");
        }

        if (context is null)
        {
            // リダイレクトサーバーからのリクエストが取得できなかった場合
            throw new Exception("No context received from the redirect server.");
        }

        // リダイレクトサーバーからのリクエストからアクセストークンを取得
        var token = context.Request.QueryString["access_token"];
        var scope = context.Request.QueryString["scope"];
        var tokenType = context.Request.QueryString["token_type"];
        var retState = context.Request.QueryString["state"];

        if (retState != state)
        {
            throw new Exception($"State mismatch: expected {state}, got {retState}");
        }

        if (string.IsNullOrEmpty(token))
        {
            // 認証コードが取得できなかった場合、次のURLの形式でリダイレクトされる
            // {redirect_url}?error={error}&error_description={error_description}&state={state}
            var error = context.Request.QueryString["error"] ?? "Unknown error";
            var errorDescription = context.Request.QueryString["error_description"] ?? "No description provided";

            throw new Exception($"Authorization failed: {error} - {errorDescription}");
        }

        uint expiresIn = 0; // Implicit flow does not provide an expiration time
        uint.TryParse(context.Request.QueryString["expires_in"], out expiresIn);

        return new TwitchOAuthToken(
            token,
            expiresIn,
            null,
            scope?.Split(_space, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            tokenType ?? "Bearer"
        );
    }

    private string CreateResponseHtml()
    {
        return $@"
        <!DOCTYPE html>
        <html>
          <head><title>Twitch認証</title></head>
          <body>
            <h1>認証が完了しました。</h1>
            <p>このページを閉じてください。</p>
          </body>
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

    public void Dispose()
    {
        RedirectServer?.Dispose();
    }
}
