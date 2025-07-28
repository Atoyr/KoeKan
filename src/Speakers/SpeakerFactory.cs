namespace Medoz.KoeKan.Speakers;

public static class SpeakerFactory
{
	public static T Create<T>(ISpeakerOptions options) where T : ISpeaker
	{
		return (T)Create(typeof(T), options);
	}

	public static ISpeaker Create(Type clientType, ISpeakerOptions options)
	{
		if (!typeof(ISpeaker).IsAssignableFrom(clientType))
		{
			throw new ArgumentException($"Type {clientType.FullName} does not implement IClient interface.", nameof(clientType));
		}

		var client = Activator.CreateInstance(clientType, options);

		if (client is null)
		{
			throw new InvalidOperationException($"Could not create instance of {clientType.FullName}.");
		}

		if (client is not ISpeaker)
		{
			throw new InvalidCastException($"Created instance of {clientType.FullName} does not implement IClient interface.");
		}

		return (ISpeaker)client;
	}
}

