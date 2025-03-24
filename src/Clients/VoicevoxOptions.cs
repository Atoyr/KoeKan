namespace Medoz.KoeKan.Clients;


public class VoicevoxOptions : IClientOptions
{
    private readonly Dictionary<string, string> _options = new();

    public uint SpeakerId
    {
        get
        {
            if(_options["speakerId"] is null)
            {
                return 0;
            }
            return uint.Parse(_options["speakerId"]);
        }
        set => _options["speakerId"] = value.ToString();
    }

    public string? Url
    {
        get
        {
            if(_options["url"] is null)
            {
                return null;
            }
            return _options["url"];
        }

        set
        {
            if(value is null)
            {
                _options.Remove("url");
            }
            else
            {
                _options["url"] = value;
            }
        }
    }

    public string this[string key]
    {
        get => _options[key];
        set => _options[key] = value;
    }
}