namespace Medoz.KoeKan.Speakers;

public interface ISpeakerOptions
{
    string this[string key] { get; set; }
    string[] BindingKeys { get; }
}
