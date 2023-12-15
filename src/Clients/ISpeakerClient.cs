namespace Medoz.MessageTransporter.Clients;

public interface ISpeakerClient: IClient
{
    Task SpeakMessageAsync(string message);
}
