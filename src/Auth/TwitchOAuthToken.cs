using System.Text.Json.Serialization;

namespace Medoz.CatChast.Auth;

/// <summary>
/// </summary>
public record TwitchOAuthToken(
    /// <summary>
    /// アクセストークン
    /// </summary>
    [property: JsonPropertyName("access_token")]
    string AccessToken,

    /// <summary>
    /// トークンの有効期限（秒）
    /// </summary>
    [property: JsonPropertyName("expires_in")]
    uint ExpiresIn,

    /// <summary>
    /// リフレッシュトークン
    /// </summary>
    [property: JsonPropertyName("refresh_token")]
    string? RefreshToken,

    /// <summary>
    /// トークンのスコープ
    /// </summary>
    [property: JsonPropertyName("scope")]
    string[] Scope,

    /// <summary>
    /// トークンのタイプ
    /// </summary>
    [property: JsonPropertyName("token_type")]
    string TokenType
);