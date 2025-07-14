
namespace Medoz.CatChast.Auth;

/// <summary>
/// </summary>
public interface ITwitchOAuth : IDisposable
{
    Task<TwitchOAuthToken> AuthorizeAsync(string? refreshToken = null, CancellationToken cancellationToken = default);
}