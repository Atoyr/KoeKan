namespace Medoz.KoeKan.Clients;

public class TwitchOptions : IClientOptions
{
    public string Uri { get => "wss://irc-ws.chat.twitch.tv:443"; }

    // Client TypeがPublicなので、ClientSecretは不要
    public readonly string ClientId = "rgl0q1gsjromlw3ro7z8n4p2g9w34u";

    public readonly string Username = "KoeKan";

    public IEnumerable<string> Channels
    {
        get
        {
            if (!_options.ContainsKey("Channels"))
            {
                return Array.Empty<string>();
            }
            return _options["Channels"].Split(',');
        }
        set
        {
            _options["Channels"] = string.Join(',', value);
        }
    }

    public string? Token
    {
        get
        {
            if (!_options.ContainsKey("Token"))
            {
                return null;
            }
            return _options["Token"];
        }
        set
        {
            if (value is null)
            {
                _options.Remove("Token");
                return;
            }
            _options["Token"] = value;
        }
    }

    private readonly Dictionary<string, string> _options = new();

    public string this[string key]
    {
        get => _options[key];
        set => _options[key] = value;
    }
}