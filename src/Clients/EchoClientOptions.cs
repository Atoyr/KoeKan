namespace Medoz.KoeKan.Clients;


public class EchoOptions : IClientOptions
{
    private readonly Dictionary<string, string> _options = new();

    public string this[string key]
    {
        get => _options[key];
        set => _options[key] = value;
    }
}
