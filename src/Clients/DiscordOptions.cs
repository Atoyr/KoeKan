namespace Medoz.KoeKan.Clients;


public class DiscordOptions : IClientOptions
{
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
            if(value is null)
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