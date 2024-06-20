namespace Medoz.KoeKan.Clients;

public record TwitchOAuthResponse(string AccessToken, ulong ExpiresIn, IEnumerable<string> Scope, string TokenType)
{
}