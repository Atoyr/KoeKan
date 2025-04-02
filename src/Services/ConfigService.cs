using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public class ConfigService
{

    private readonly Config _config;

    public ConfigService()
    {
    }

    public ConfigService(Config config)
    {
        _config = config;
    }

    public Config GetConfig()
    {
        return _config;
    }

    public void SaveConfig()
    {
        _config.Save();
    }
}

