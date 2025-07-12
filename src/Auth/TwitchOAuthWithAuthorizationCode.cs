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
public class TwitchOAuthWithClientCredentials : TwitchOAuthBase, IDisposable
{
    /// <summary>
    /// スペースのURLエンコード
    /// </summary>
    private const string _space = "%20";

    internal TwitchOAuthOptions Options { get; }

    private readonly string _clientSecret;

    public event Action<string>? OnTokenReceived;


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

    public TwitchOAuthWithClientCredentials(TwitchOAuthOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _clientId = options.ClientId;
        _redirectPort = options.RedirectPort;
        _clientSecret = options["client_secret"] as string ?? throw new ArgumentNullException("Client secret is not set.");
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
            var code = await GetAuthorizeCodeAsync();
            if (string.IsNullOrEmpty(code))
            {
                throw new Exception("Authorization code is empty.");
            }
            return await GetTokenAsync(code, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to authorize with Twitch OAuth.", ex);
        }
    }

    private async Task<HttpListenerContext?> RequestTwichAutorize(string state, CancellationToken cancellationToken = default)
    {
        string url = $"{OAuthAuthorizeUri}?client_id={_clientId}&redirect_uri={_redirectUrl}&response_type=code&scope={Scope}&state={state}";

        // リダイレクトサーバーを起動して、認証コードを取得する
        HttpListenerContext? context = null;
        try
        {
            using var server = new RedirectServer(_redirectUrl);
            var redirectTask = server.GetContextAsync();

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
                context = await redirectTask;

                if (context is null)
                {
                    // リダイレクトサーバーからのリクエストがまだ来ていない場合は、再度待機
                    redirectTask = server.GetContextAsync();
                }
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

    private async Task<string> GetAuthorizeCodeAsync(CancellationToken cancellationToken = default)
    {
        string state = Guid.NewGuid().ToString("N");
        var context = await RequestTwichAutorize(state, cancellationToken);

        if (context is null)
        {
            // リダイレクトサーバーからのリクエストが取得できなかった場合
            throw new Exception("No context received from the redirect server.");
        }

        // リダイレクトサーバーからのリクエストから認証コードを取得
        var code = context.Request.QueryString["code"];
        var retState = context.Request.QueryString["state"];

        if (retState != state)
        {
            throw new Exception($"State mismatch: expected {state}, got {retState}");
        }

        if (string.IsNullOrEmpty(code))
        {
            // 認証コードが取得できなかった場合、次のURLの形式でリダイレクトされる
            // {redirect_url}?error={error}&error_description={error_description}&state={state}
            var error = context.Request.QueryString["error"] ?? "Unknown error";
            var errorDescription = context.Request.QueryString["error_description"] ?? "No description provided";

            throw new Exception($"Authorization failed: {error} - {errorDescription}");
        }

        return code;
    }

    private async Task<TwitchOAuthToken> GetTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException("Authorization code cannot be null or empty.", nameof(code));
        }

        using var http = new HttpClient();
        var tokenResponse = await http.PostAsync(OAuthAuthorizeUri, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"client_id", _clientId},
            {"client_secret", _clientSecret},
            {"code", code},
            {"grant_type", "authorization_code"},
            {"redirect_uri", _redirectUrl}
        }));

        string json = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<TwitchOAuthToken>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return tokenData ?? throw new Exception("Failed to deserialize Twitch OAuth token response.");
    }

    public void Dispose()
    {
    }
}
