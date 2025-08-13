namespace Depot.Auth.Domain.Users;

using System.Text;
using Auth;
using ErrorOr;

public record Username
{
    internal Username(string value)
    {
        Value = value.Trim();
        Normalized = Value
            .Normalize(NormalizationForm.FormKC)
            .ToLowerInvariant();
    }

    public UserId? UserId { get; set; }

    public string Value { get; } = null!;

    public string Normalized { get; } = null!;

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
        return new Username(username);
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