
namespace Medoz.CatChast.Auth;

/// <summary>
/// </summary>
public interface ITwitchOAuth
{
    Task<TwitchOAuthToken> AuthorizeAsync(string? refreshToken = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default);
}