using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace Medoz.CatChast.Auth;

/// <summary>
/// </summary>
public class TwitchOAuthOptions
{
    private readonly Dictionary<string, object> _parameters = new();

    /// <summary>
    /// Twitch OAuthのクライアントID
    /// </summary>
    public string ClientId
    {
        get => this["client_id"] as string ?? string.Empty;
    }

    /// <summary>
    /// リダイレクトサーバーのポート番号
    /// </summary>
    public int RedirectPort
    {
        get => _parameters.TryGetValue("redirect_port", out var value) ? Convert.ToInt32(value) : 8080;
    }

    /// <summary>
    /// スペースのURLエンコード
    /// </summary>
    private const string _space = "%20";

    // FIXME: スコープを外側から柔軟に設定できるようにする
    /// <summary>
    /// Twitch OAuthのスコープ
    /// </summary>
    public string Scope => $"chat:read{_space}chat:edit";

    public object? this[string key]
    {
        get => _parameters.TryGetValue(key, out var value) ? value : null;
        set => _parameters[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool TryGetStringValue(string key, out string value)
    {
        if (_parameters.TryGetValue(key, out var objValue) && objValue is string strValue)
        {
            value = strValue;
            return true;
        }
        value = string.Empty;
        return false;
    }

    public string GetStringValue(string key)
    {
        if (TryGetStringValue(key, out var value))
        {
            return value;
        }
        throw new KeyNotFoundException($"Key '{key}' not found in TwitchOAuthOptions.");
    }

    public TwitchOAuthOptions(string clientId, int redirectPort = 8080)
    {
        _parameters["client_id"] = clientId;
        _parameters["redirect_port"] = redirectPort;
    }
}