namespace Medoz.KoeKan.Speakers;

public static class SpeakerFactory
{
	public static T Create<T>(ISpeakerOptions options) where T : ISpeaker
	{
		return (T)Create(typeof(T), options);
	}

	public static ISpeaker Create(Type speakerType, ISpeakerOptions options)
	{
		if (!typeof(ISpeaker).IsAssignableFrom(speakerType))
		{
			throw new ArgumentException($"Type {speakerType.FullName} does not implement ISpeaker interface.", nameof(speakerType));
		}

		var speaker = Activator.CreateInstance(speakerType, options);

		if (speaker is null)
		{
			throw new InvalidOperationException($"Could not create instance of {speakerType.FullName}.");
		}

		if (speaker is not ISpeaker)
		{
			throw new InvalidCastException($"Created instance of {speakerType.FullName} does not implement ISpeaker interface.");
		}

		return (ISpeaker)speaker;
	}
}

