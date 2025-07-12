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

    public abstract Task<TwitchOAuthToken> AuthorizeAsync(string? refreshToken, CancellationToken cancellationToken = default);

    public async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");

        var response = await client.GetAsync(OAuthValidateUri, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}