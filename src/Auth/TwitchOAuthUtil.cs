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
public static class TwitchOAuthUtil
{
    /// <summary>
    /// Twitch OAuthのトークン検証用URI
    /// </summary>
    internal const string OAuthValidateUri = "https://id.twitch.tv/oauth2/validate";

    private static HttpClient? _httpClient;

    public static async Task<bool> ValidateTokenAsync(string? accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return false;
        }

        if (_httpClient is null)
        {
            _httpClient = new HttpClient();
        }
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");

        var response = await _httpClient.GetAsync(OAuthValidateUri, cancellationToken).ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}
