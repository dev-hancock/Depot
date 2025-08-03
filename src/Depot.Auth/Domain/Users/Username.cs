namespace Depot.Auth.Domain.Users;

using System.Text;
using Auth;
using ErrorOr;

public record Username
{
    private Username() { }

    public UserId? UserId { get; set; }

    public string Value { get; private set; } = null!;

    public string Normalized { get; private set; } = null!;

    public virtual bool Equals(Username? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Normalized, other.Normalized, StringComparison.Ordinal);
    }

    public static Username Create(string username)
    {
        var value = username.Trim();

        var normalized = value
            .Normalize(NormalizationForm.FormKC)
            .ToLowerInvariant();

        return new Username
        {
            Value = value,
            Normalized = normalized
        };
    }

    public static ErrorOr<Username> TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation();
        }

        return Create(value);
    }

    public static implicit operator string(Username email)
    {
        return email.Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Normalized);
    }
}