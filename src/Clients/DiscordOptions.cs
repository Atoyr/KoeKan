namespace Medoz.KoeKan.Clients;


public class DiscordOptions : IClientOptions
{
    public string Token
    {
        get
        {
            return _options["Token"];
        }
        set
        {
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