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

    /// <summary>
    /// Twitch OAuthのトークン検証用URI
    /// </summary>
    protected const string OAuthValidateUri = "https://id.twitch.tv/oauth2/validate";

    protected HttpClient? _httpClient;

    public abstract Task<TwitchOAuthToken> AuthorizeAsync(string? refreshToken, CancellationToken cancellationToken = default);

    public async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (_httpClient is null)
        {
            _httpClient = new HttpClient();
        }
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");

        var response = await _httpClient.GetAsync(OAuthValidateUri, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public virtual void Dispose()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }
}