namespace Depot.Auth.Domain;

public sealed class User
{
    public Guid Id { get; init; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public ICollection<UserRole> UserRoles { get; init; } = new List<UserRole>();

    public ICollection<Token> Tokens { get; init; } = new List<Token>();

    public TokenPair IssueToken(ISecureRandom random, TimeProvider time, ITokenGenerator tokens, TimeSpan lifetime)
    {
        var now = time.GetUtcNow().UtcDateTime;

        var refresh = RefreshToken.New(random);

        var pair = new TokenPair
        {
            Access = tokens.CreateAccessToken(this, now),
            Refresh = refresh
        };

        Tokens.Add(Token.Refresh(this, refresh, now, lifetime));

        return pair;
    }
}