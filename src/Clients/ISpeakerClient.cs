namespace Medoz.KoeKan.Clients;

public interface ISpeakerClient: IClient
{
    Task SpeakMessageAsync(string message);
}