namespace Medoz.KoeKan.Speakers;

public interface ISpeakerOptions
{
    string this[string key] { get; set; }
    IEnumerable<string> BindingKeys { get; }
}
