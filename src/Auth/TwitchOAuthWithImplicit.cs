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

    public TwitchOAuthWithImplicit(TwitchOAuthOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _clientId = options.ClientId;
        _redirectPort = options.RedirectPort;
    }


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

    private async Task<HttpListenerContext?> RequestTwichAutorize(string state, CancellationToken cancellationToken = default)
    {
        // response_type=token
        string url = $"{OAuthAuthorizeUri}?client_id={_clientId}&redirect_uri={_redirectUrl}&response_type=token&scope={Scope}&state={state}";
        using var server = new RedirectServer(_redirectUrl);
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

            while (context is null && !cancellationToken.IsCancellationRequested)
            {
                // リダイレクトサーバーからのリクエストを待機
                context = await server.GetContextAsync();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get authorization code.", ex);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            // キャンセルされた場合はnullを返す
            return null;
        }
        return context;
    }

    private async Task<TwitchOAuthToken> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        string state = Guid.NewGuid().ToString("N");
        var context = await RequestTwichAutorize(state, cancellationToken);

        if (context is null)
        {
            // リダイレクトサーバーからのリクエストが取得できなかった場合
            throw new Exception("No context received from the redirect server.");
        }

        // リダイレクトサーバーからのリクエストからアクセストークンを取得
        var token = context.Request.QueryString["access_token"];
        var scope = context.Request.QueryString["scope"];
        var retState = context.Request.QueryString["state"];
        var tokenType = context.Request.QueryString["token_type"];

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

        return new TwitchOAuthToken(
            token,
            0,
            null,
            scope?.Split(_space, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            tokenType ?? "Bearer"
        );
    }

    public void Dispose()
    {
    }
}
