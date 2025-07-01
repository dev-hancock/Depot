namespace Depot.Auth.Domain;

public sealed class TokenPair
{
    public string Access { get; init; } = null!;

    public RefreshToken Refresh { get; init; } = null!;
}