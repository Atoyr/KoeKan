
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public interface IConfigService
{
    Config GetConfig();
    void SaveConfig();

    Secret GetSecret();
    void SaveSecret();
}
