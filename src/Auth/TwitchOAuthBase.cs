using System.Net.Http;

namespace Medoz.CatChast.Auth;

public abstract class TwitchOAuthBase : ITwitchOAuth
{
    /// <summary>
    /// Twitch OAuthのトークン取得用URI
    /// </summary>
    protected const string OAuthTokenUri = "https://id.twitch.tv/oauth2/token";


    /// <summary>
    /// Twitch OAuthの認証用URI
    /// </summary>
    protected const string OAuthAuthorizeUri = "https://id.twitch.tv/oauth2/authorize";

    protected HttpClient? _httpClient;

    public abstract Task<TwitchOAuthToken> AuthorizeAsync(string? refreshToken, CancellationToken cancellationToken = default);

    public virtual void Dispose()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }
}